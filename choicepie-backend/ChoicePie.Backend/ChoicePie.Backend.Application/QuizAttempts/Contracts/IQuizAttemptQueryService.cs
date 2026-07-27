using ChoicePie.Backend.Application.QuizAttempts.Dtos;

namespace ChoicePie.Backend.Application.QuizAttempts.Contracts;

public interface IQuizAttemptQueryService
{
    Task<QuizAttemptResultDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    // Completed attempts only, newest first - an in-progress attempt has no CompletedAt/duration
    // to show in a history list.
    Task<IReadOnlyList<QuizAttemptHistoryItemDto>> ListHistoryAsync(
        Guid quizId, Guid memberId, CancellationToken cancellationToken);
}
