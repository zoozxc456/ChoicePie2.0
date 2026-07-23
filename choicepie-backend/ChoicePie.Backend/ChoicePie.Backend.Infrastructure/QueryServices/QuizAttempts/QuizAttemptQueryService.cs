using ChoicePie.Backend.Application.QuizAttempts.Contracts;
using ChoicePie.Backend.Application.QuizAttempts.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Enums;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using QuizAttemptAggregate = ChoicePie.Backend.Domain.Aggregates.QuizAttempt.QuizAttempt;

namespace ChoicePie.Backend.Infrastructure.QueryServices.QuizAttempts;

public sealed class QuizAttemptQueryService(IReadRepository readRepository) : IQuizAttemptQueryService, IScopedDependency
{
    public Task<QuizAttemptResultDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var joined =
            (from a in readRepository.Query<QuizAttemptAggregate>()
             where a.Id == id
             join q in readRepository.Query<Quiz>() on a.QuizId equals q.Id into quizGroup
             from quiz in quizGroup.DefaultIfEmpty()
             select new { Attempt = a, Quiz = quiz })
            .FirstOrDefault();

        if (joined is null)
        {
            return Task.FromResult<QuizAttemptResultDto?>(null);
        }

        var attempt = joined.Attempt;
        var owningQuiz = joined.Quiz;

        var isInProgress = attempt.Status == AttemptStatus.InProgress;
        var selectedOptionByQuestionId = attempt.Answers.ToDictionary(a => a.QuestionId, a => a.SelectedOptionIndex);
        var questions = owningQuiz?.Questions ?? [];
        var answers = questions
            .Select(question =>
            {
                var selected = selectedOptionByQuestionId.GetValueOrDefault(question.Id, -1);
                return new QuizAttemptAnswerResultDto(
                    question.Id,
                    question.Text,
                    selected == -1 ? null : selected,
                    isInProgress ? null : question.AnswerIndex,
                    !isInProgress && selected == question.AnswerIndex,
                    isInProgress ? null : question.Explanation);
            })
            .ToList();

        var dto = new QuizAttemptResultDto(
            attempt.Id, attempt.QuizId, owningQuiz?.Title ?? "Unknown", attempt.MemberId, attempt.Score,
            attempt.Passed, attempt.StartedAt, attempt.CompletedAt, answers);

        return Task.FromResult<QuizAttemptResultDto?>(dto);
    }
}
