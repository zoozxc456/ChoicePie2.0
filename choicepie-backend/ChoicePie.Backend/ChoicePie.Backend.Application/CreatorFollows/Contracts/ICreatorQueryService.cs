using ChoicePie.Backend.Application.CreatorFollows.Dtos;

namespace ChoicePie.Backend.Application.CreatorFollows.Contracts;

public interface ICreatorQueryService
{
    Task<CreatorProfileDto?> GetByIdAsync(Guid creatorId, Guid? currentUserId, CancellationToken cancellationToken);
}
