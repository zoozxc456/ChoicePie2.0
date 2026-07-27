using ChoicePie.Backend.Application.CreatorFollows.Commands;
using ChoicePie.Backend.Domain.Aggregates.CreatorFollow;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;
using CreatorFollowAggregate = ChoicePie.Backend.Domain.Aggregates.CreatorFollow.CreatorFollow;

namespace ChoicePie.Backend.Application.Tests.CreatorFollows;

[TestFixture]
public class RemoveCreatorFollowCommandHandlerTests
{
    private ICreatorFollowRepository _followRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private RemoveCreatorFollowCommandHandler _sut = null!;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _creatorId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _followRepository = Substitute.For<ICreatorFollowRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new RemoveCreatorFollowCommandHandler(_followRepository, _currentUserService, _unitOfWork);

        _currentUserService.UserId.Returns(_userId);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenExistingFollow_WhenCalled_ThenDeletesAndPersists()
    {
        var follow = CreatorFollowAggregate.Create(_userId, _creatorId);
        _followRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<CreatorFollowAggregate>>(), Arg.Any<CancellationToken>())
            .Returns(follow);

        await _sut.Handle(new RemoveCreatorFollowCommand(_creatorId), CancellationToken.None);

        await _followRepository.Received(1).DeleteAsync(follow, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenNoExistingFollow_WhenCalled_ThenDoesNothing()
    {
        _followRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<CreatorFollowAggregate>>(), Arg.Any<CancellationToken>())
            .Returns((CreatorFollowAggregate?)null);

        await _sut.Handle(new RemoveCreatorFollowCommand(_creatorId), CancellationToken.None);

        await _followRepository.DidNotReceive().DeleteAsync(Arg.Any<CreatorFollowAggregate>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenUnauthenticatedUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() => _sut.Handle(new RemoveCreatorFollowCommand(_creatorId), CancellationToken.None));
    }
}
