using ChoicePie.Backend.Application.CreatorFollows.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;

namespace ChoicePie.Backend.Application.CreatorFollows.Contracts;

public interface ICreatorQueryService
{
    Task<CreatorProfileDto?> GetByIdAsync(Guid creatorId, Guid? currentUserId, CancellationToken cancellationToken);

    // 會員追蹤中的創作者，依追蹤時間新到舊。
    Task<PagedResult<FollowedCreatorDto>> ListFollowingAsync(
        Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);
}
