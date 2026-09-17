using ChoicePie.Backend.Application.Quizzes.Commands;

namespace ChoicePie.Backend.WebApi.Requests.Quizzes;

public sealed record UpdateQuizCoverRequest(string? CoverImageUrl, string CoverEmoji, string CoverGradient)
{
    public UpdateQuizCoverCommand ToCommand(Guid id) => new()
    {
        Id = id,
        CoverImageUrl = CoverImageUrl,
        CoverEmoji = CoverEmoji,
        CoverGradient = CoverGradient
    };
}
