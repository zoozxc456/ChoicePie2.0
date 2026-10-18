using ChoicePie.Backend.Application.GameRooms;
using ChoicePie.Backend.Application.GameRooms.Commands;
using ChoicePie.Backend.Application.GameRooms.Queries;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.GameRooms;

[TestFixture]
public class GameRoomLockBehaviorTests
{
    private IDistributedLock _distributedLock = null!;
    private LeaseState _lease = null!;

    [SetUp]
    public void SetUp()
    {
        _lease = new LeaseState();
        _distributedLock = Substitute.For<IDistributedLock>();
        _distributedLock.AcquireAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult<IAsyncDisposable>(new TrackingLease(_lease)));
    }

    [Test]
    public async Task Handle_GivenGameRoomCommand_WhenCalled_ThenRunsHandlerWhileHoldingRoomLock()
    {
        var sut = new GameRoomLockBehavior<PauseGameCommand, string>(_distributedLock);
        var leaseReleasedDuringHandler = true;

        var result = await sut.Handle(new PauseGameCommand("ABC123", Guid.NewGuid()), _ =>
        {
            leaseReleasedDuringHandler = _lease.Released;
            return Task.FromResult("done");
        }, CancellationToken.None);

        Assert.That(result, Is.EqualTo("done"));
        await _distributedLock.Received(1).AcquireAsync("GameRoom:ABC123", Arg.Any<CancellationToken>());
        Assert.That(leaseReleasedDuringHandler, Is.False, "handler 執行期間必須持有鎖");
        Assert.That(_lease.Released, Is.True, "handler 結束後要釋放鎖");
    }

    [Test]
    public void Handle_GivenGameRoomCommand_WhenHandlerThrows_ThenStillReleasesLock()
    {
        var sut = new GameRoomLockBehavior<PauseGameCommand, string>(_distributedLock);

        Assert.ThrowsAsync<InvalidOperationException>(() => sut.Handle(
            new PauseGameCommand("ABC123", Guid.NewGuid()),
            _ => throw new InvalidOperationException(),
            CancellationToken.None));

        Assert.That(_lease.Released, Is.True);
    }

    [Test]
    public async Task Handle_GivenNonGameRoomRequest_WhenCalled_ThenSkipsLock()
    {
        var sut = new GameRoomLockBehavior<WatchRoomQuery, string>(_distributedLock);

        var result = await sut.Handle(new WatchRoomQuery("ABC123"), _ => Task.FromResult("done"), CancellationToken.None);

        Assert.That(result, Is.EqualTo("done"));
        await _distributedLock.DidNotReceiveWithAnyArgs().AcquireAsync(default!, default);
    }

    /// <summary>
    /// 「讀取 → 修改 → 整份寫回」的房間指令都必須序列化；新增指令時忘了標記會在這裡失敗。
    /// CreateRoom 例外：房間還不存在，沒有可以競爭的舊狀態。
    /// </summary>
    [Test]
    public void GameRoomCommands_GivenEveryCommandThatMutatesARoom_ThenImplementsIGameRoomCommand()
    {
        var commandTypes = typeof(IGameRoomCommand).Assembly.GetTypes()
            .Where(t => t.Namespace == typeof(PauseGameCommand).Namespace)
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)))
            .Where(t => t != typeof(CreateRoomCommand))
            .ToList();

        Assert.That(commandTypes, Is.Not.Empty);
        Assert.That(
            commandTypes.Where(t => !typeof(IGameRoomCommand).IsAssignableFrom(t)).Select(t => t.Name),
            Is.Empty);
    }

    private sealed class LeaseState
    {
        public bool Released { get; set; }
    }

    private sealed class TrackingLease(LeaseState state) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            state.Released = true;
            return ValueTask.CompletedTask;
        }
    }
}
