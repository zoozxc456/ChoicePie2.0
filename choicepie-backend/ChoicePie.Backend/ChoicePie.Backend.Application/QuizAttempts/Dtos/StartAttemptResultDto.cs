using ChoicePie.Backend.Application.Quizzes.Dtos;

namespace ChoicePie.Backend.Application.QuizAttempts.Dtos;

public sealed record StartAttemptResultDto(Guid AttemptId, QuizForAttemptDto Quiz);
