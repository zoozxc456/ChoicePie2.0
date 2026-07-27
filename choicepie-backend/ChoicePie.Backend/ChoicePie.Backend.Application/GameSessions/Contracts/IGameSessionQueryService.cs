using ChoicePie.Backend.Application.GameSessions.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;

namespace ChoicePie.Backend.Application.GameSessions.Contracts;

public interface IGameSessionQueryService
{
    Task<PagedResult<GameSessionSummaryDto>> GetHostedByUserIdAsync(
        Guid hostUserId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<PagedResult<GameSessionSummaryDto>> GetPlayedByMemberIdAsync(
        Guid memberId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<GameSessionDetailDto?> GetByIdAsync(Guid id, Guid requestingUserId, CancellationToken cancellationToken);
}
