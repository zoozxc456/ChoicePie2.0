using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Commands;

public sealed class RecordQuizShareCommandHandler(IQuizRepository quizRepository)
    : IRequestHandler<RecordQuizShareCommand, int>
{
    public async Task<int> Handle(RecordQuizShareCommand request, CancellationToken cancellationToken) =>
        await quizRepository.IncrementShareCountAsync(request.Id, cancellationToken)
        ?? throw new QuizNotFoundException(request.Id);
}
