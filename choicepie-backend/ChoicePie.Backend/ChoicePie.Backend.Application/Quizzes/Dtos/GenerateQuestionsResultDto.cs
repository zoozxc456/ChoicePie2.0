namespace ChoicePie.Backend.Application.Quizzes.Dtos;

public sealed record GeneratedQuestionDto(string Text, IReadOnlyList<string> Options, int AnswerIndex, string Explanation);

public sealed record GenerateQuestionsResultDto(IReadOnlyList<GeneratedQuestionDto> Questions, int TokensUsed);
