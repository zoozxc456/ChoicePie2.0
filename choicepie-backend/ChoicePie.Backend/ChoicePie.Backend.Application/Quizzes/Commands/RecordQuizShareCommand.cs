using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Commands;

public sealed record RecordQuizShareCommand(Guid Id) : IRequest<int>;
