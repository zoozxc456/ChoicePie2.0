using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.QuizAttempts.Commands;

public sealed class SubmitQuizAttemptAnswerCommandHandler(
    IQuizAttemptRepository quizAttemptRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SubmitQuizAttemptAnswerCommand>
{
    public async Task Handle(SubmitQuizAttemptAnswerCommand request, CancellationToken cancellationToken)
    {
        var memberId = currentUserService.UserId ?? throw new UnauthenticatedException();
        var attempt = await quizAttemptRepository.GetByIdAsync(request.AttemptId, cancellationToken)
                      ?? throw new QuizAttemptNotFoundException(request.AttemptId);

        attempt.EnsureOwnedBy(memberId);
        attempt.SubmitAnswer(request.QuestionId, request.SelectedOptionIndex, DateTime.UtcNow);

        await quizAttemptRepository.UpdateAsync(attempt, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
