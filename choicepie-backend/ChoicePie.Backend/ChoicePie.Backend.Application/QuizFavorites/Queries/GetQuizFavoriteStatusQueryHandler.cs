using ChoicePie.Backend.Domain.Aggregates.QuizFavorite;
using ChoicePie.Backend.Domain.Aggregates.QuizFavorite.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.QuizFavorites.Queries;

public sealed class GetQuizFavoriteStatusQueryHandler(
    IQuizFavoriteRepository favoriteRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetQuizFavoriteStatusQuery, bool>
{
    public async Task<bool> Handle(GetQuizFavoriteStatusQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        if (userId is null)
        {
            return false;
        }

        return await favoriteRepository.ExistsAsync(
            new QuizFavoriteByUserAndQuizSpecification(userId.Value, request.QuizId), cancellationToken);
    }
}
