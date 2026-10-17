using ChoicePie.Backend.Application.QuizAttempts.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.QuizAttempts.Commands;

public sealed class CompleteQuizAttemptCommandHandler(
    IQuizAttemptRepository quizAttemptRepository,
    IQuizRepository quizRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<CompleteQuizAttemptCommand, QuizAttemptResultDto>
{
    public async Task<QuizAttemptResultDto> Handle(CompleteQuizAttemptCommand request, CancellationToken cancellationToken)
    {
        var memberId = currentUserService.UserId ?? throw new UnauthenticatedException();
        var attempt = await quizAttemptRepository.GetByIdAsync(request.AttemptId, cancellationToken)
                      ?? throw new QuizAttemptNotFoundException(request.AttemptId);

        attempt.EnsureOwnedBy(memberId);

        var quiz = await quizRepository.GetByIdAsync(attempt.QuizId, cancellationToken)
                   ?? throw new QuizNotFoundException(attempt.QuizId);

        var correctAnswerIndexByQuestionId = quiz.Questions.ToDictionary(q => q.Id, q => q.AnswerIndex);
        attempt.Complete(correctAnswerIndexByQuestionId, timeProvider.GetUtcNow().UtcDateTime);

        await quizAttemptRepository.UpdateAsync(attempt, cancellationToken);
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            // 連點「繳交」時另一個請求已完成這次作答；挑戰次數只會由先完成的那次計算。
            throw new InvalidQuizAttemptStateException(attempt.Id, "作答已經結束，無法重複繳交。");
        }

        var selectedOptionByQuestionId = attempt.Answers.ToDictionary(a => a.QuestionId, a => a.SelectedOptionIndex);
        var answers = quiz.Questions.Select(question =>
        {
            var selected = selectedOptionByQuestionId.GetValueOrDefault(question.Id, -1);
            return new QuizAttemptAnswerResultDto(
                question.Id,
                question.Text,
                selected == -1 ? null : selected,
                question.AnswerIndex,
                selected == question.AnswerIndex,
                question.Explanation);
        }).ToList();

        return new QuizAttemptResultDto(
            attempt.Id, quiz.Id, quiz.Title, attempt.MemberId, attempt.Score, attempt.Passed,
            attempt.StartedAt, attempt.CompletedAt, answers);
    }
}
