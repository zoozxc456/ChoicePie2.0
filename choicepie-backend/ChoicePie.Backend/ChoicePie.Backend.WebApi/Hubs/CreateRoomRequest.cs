namespace ChoicePie.Backend.WebApi.Hubs;

/// <summary>對應 spec 2.1 的 `CreateRoom` client→server 參數：{ quizId?, questionIds, timeLimit? }。</summary>
public sealed record CreateRoomRequest(Guid QuizId, IReadOnlyList<Guid>? QuestionIds, int? TimeLimit);
