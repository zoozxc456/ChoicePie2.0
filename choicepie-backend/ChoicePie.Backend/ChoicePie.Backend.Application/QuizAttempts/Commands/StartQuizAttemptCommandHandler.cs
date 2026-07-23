using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Application.QuizAttempts.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;
using QuizAttemptAggregate = ChoicePie.Backend.Domain.Aggregates.QuizAttempt.QuizAttempt;

namespace ChoicePie.Backend.Application.QuizAttempts.Commands;

public sealed class StartQuizAttemptCommandHandler(
    IQuizRepository quizRepository,
    IQuizAttemptRepository quizAttemptRepository,
    IMemberRepository memberRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<StartQuizAttemptCommand, StartAttemptResultDto>
{
    public async Task<StartAttemptResultDto> Handle(StartQuizAttemptCommand request, CancellationToken cancellationToken)
    {
        var memberId = currentUserService.UserId ?? throw new UnauthenticatedException();
        var quiz = await quizRepository.GetByIdAsync(request.QuizId, cancellationToken)
                   ?? throw new QuizNotFoundException(request.QuizId);

        if (quiz.Status != QuizStatus.Published)
        {
            throw new QuizNotPublishedException(quiz.Id);
        }

        var existingAttempt = await quizAttemptRepository.FirstOrDefaultAsync(
            new QuizAttemptInProgressByQuizAndMemberSpecification(quiz.Id, memberId), cancellationToken);

        if (existingAttempt is null)
        {
            var questionIds = quiz.Questions.Select(q => q.Id).ToList();
            existingAttempt = QuizAttemptAggregate.Start(quiz.Id, memberId, questionIds, timeProvider.GetUtcNow().UtcDateTime);

            await quizAttemptRepository.AddAsync(existingAttempt, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var attempt = existingAttempt;
        var creator = await memberRepository.GetByIdAsync(quiz.OwnerId, cancellationToken);
        var quizDto = QuizForAttemptDto.FromDomain(quiz, creator?.Name ?? "Unknown", creator?.Avatar);

        return new StartAttemptResultDto(attempt.Id, quizDto);
    }
}
