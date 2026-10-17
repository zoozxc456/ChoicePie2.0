using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms;

/// <summary>
/// 房間狀態存在 Redis，為「讀取 → 修改 → 整份寫回」，沒有樂觀並行控制。
/// 玩家同時作答、斷線與快速重連都會幾乎同時改寫同一份房間，且可能落在不同的後端執行個體上，
/// 所以每個 <see cref="IGameRoomCommand"/> 都在房間的分散式鎖內執行，避免後寫的覆蓋先寫的。
/// </summary>
public sealed class GameRoomLockBehavior<TRequest, TResponse>(IDistributedLock distributedLock)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IGameRoomCommand command)
        {
            return await next(cancellationToken);
        }

        await using var _ = await distributedLock.AcquireAsync($"GameRoom:{command.RoomCode}", cancellationToken);
        return await next(cancellationToken);
    }
}
