using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.AdminQuizzes.Commands;

public sealed class AdminTakeDownQuizCommandHandler(
    IQuizRepository quizRepository,
    ICurrentAdminUserService currentAdminUserService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<AdminTakeDownQuizCommand>
{
    public async Task Handle(AdminTakeDownQuizCommand request, CancellationToken cancellationToken)
    {
        var adminUserId = currentAdminUserService.AdminUserId ?? throw new UnauthenticatedException();
        var quiz = await quizRepository.GetByIdAsync(request.QuizId, cancellationToken)
                   ?? throw new QuizNotFoundException(request.QuizId);

        quiz.TakeDown(adminUserId, request.Reason, timeProvider.GetUtcNow().UtcDateTime);

        await quizRepository.UpdateAsync(quiz, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
