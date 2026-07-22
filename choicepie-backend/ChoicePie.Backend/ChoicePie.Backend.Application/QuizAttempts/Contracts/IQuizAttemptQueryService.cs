using ChoicePie.Backend.Application.QuizAttempts.Dtos;

namespace ChoicePie.Backend.Application.QuizAttempts.Contracts;

public interface IQuizAttemptQueryService
{
    Task<QuizAttemptResultDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
