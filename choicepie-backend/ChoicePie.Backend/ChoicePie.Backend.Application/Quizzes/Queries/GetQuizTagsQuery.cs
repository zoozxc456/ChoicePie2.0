using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Queries;

public sealed record GetQuizTagsQuery : IRequest<IReadOnlyList<string>>;
