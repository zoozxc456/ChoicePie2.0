using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.QuizFavorite;
using ChoicePie.Backend.Domain.Aggregates.QuizFavorite.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.QuizFavorites.Commands;

public sealed class RemoveQuizFavoriteCommandHandler(
    IQuizFavoriteRepository favoriteRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveQuizFavoriteCommand>
{
    public async Task Handle(RemoveQuizFavoriteCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthenticatedException();

        var favorite = await favoriteRepository.FirstOrDefaultAsync(
            new QuizFavoriteByUserAndQuizSpecification(userId, request.QuizId), cancellationToken);

        if (favorite is null)
        {
            return;
        }

        await favoriteRepository.DeleteAsync(favorite, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
