using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Commands;

public sealed class RecordQuizShareCommandHandler(IQuizRepository quizRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<RecordQuizShareCommand, int>
{
    public async Task<int> Handle(RecordQuizShareCommand request, CancellationToken cancellationToken)
    {
        var quiz = await quizRepository.GetByIdAsync(request.Id, cancellationToken)
                   ?? throw new QuizNotFoundException(request.Id);

        quiz.RecordShare();

        await quizRepository.UpdateAsync(quiz, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return quiz.ShareCount;
    }
}
