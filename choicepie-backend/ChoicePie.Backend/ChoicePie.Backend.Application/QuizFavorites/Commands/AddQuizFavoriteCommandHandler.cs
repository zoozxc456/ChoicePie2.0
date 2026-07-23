using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.QuizFavorite;
using ChoicePie.Backend.Domain.Aggregates.QuizFavorite.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.QuizFavorites.Commands;

public sealed class AddQuizFavoriteCommandHandler(
    IQuizFavoriteRepository favoriteRepository,
    IQuizRepository quizRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddQuizFavoriteCommand>
{
    public async Task Handle(AddQuizFavoriteCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthenticatedException();

        _ = await quizRepository.GetByIdAsync(request.QuizId, cancellationToken)
            ?? throw new QuizNotFoundException(request.QuizId);

        var alreadyFavorited = await favoriteRepository.ExistsAsync(
            new QuizFavoriteByUserAndQuizSpecification(userId, request.QuizId), cancellationToken);

        if (alreadyFavorited)
        {
            return;
        }

        var favorite = QuizFavorite.Create(request.QuizId, userId);
        await favoriteRepository.AddAsync(favorite, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
