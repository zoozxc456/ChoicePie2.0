using ChoicePie.Backend.Application.CreatorFollows.Commands;
using ChoicePie.Backend.Domain.Aggregates.CreatorFollow;
using ChoicePie.Backend.Domain.Aggregates.CreatorFollow.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;
using CreatorFollowAggregate = ChoicePie.Backend.Domain.Aggregates.CreatorFollow.CreatorFollow;

namespace ChoicePie.Backend.Application.Tests.CreatorFollows;

[TestFixture]
public class AddCreatorFollowCommandHandlerTests
{
    private ICreatorFollowRepository _followRepository = null!;
    private IMemberRepository _memberRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private AddCreatorFollowCommandHandler _sut = null!;
    private readonly Guid _userId = Guid.NewGuid();
    private Member _creator = null!;

    [SetUp]
    public void SetUp()
    {
        _followRepository = Substitute.For<ICreatorFollowRepository>();
        _memberRepository = Substitute.For<IMemberRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new AddCreatorFollowCommandHandler(_followRepository, _memberRepository, _currentUserService, _unitOfWork);

        _creator = Member.Create("Creator");
        _memberRepository.GetByIdAsync(_creator.Id, Arg.Any<CancellationToken>()).Returns(_creator);
        _currentUserService.UserId.Returns(_userId);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenNotAlreadyFollowing_WhenCalled_ThenAddsFollowAndPersists()
    {
        _followRepository.ExistsAsync(Arg.Any<ISpecification<CreatorFollowAggregate>>(), Arg.Any<CancellationToken>()).Returns(false);

        await _sut.Handle(new AddCreatorFollowCommand(_creator.Id), CancellationToken.None);

        await _followRepository.Received(1).AddAsync(Arg.Any<CreatorFollowAggregate>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenAlreadyFollowing_WhenCalled_ThenDoesNotAddOrPersistAgain()
    {
        _followRepository.ExistsAsync(Arg.Any<ISpecification<CreatorFollowAggregate>>(), Arg.Any<CancellationToken>()).Returns(true);

        await _sut.Handle(new AddCreatorFollowCommand(_creator.Id), CancellationToken.None);

        await _followRepository.DidNotReceive().AddAsync(Arg.Any<CreatorFollowAggregate>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenCreatorNotFound_WhenCalled_ThenThrowsMemberNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _memberRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((Member?)null);

        Assert.ThrowsAsync<MemberNotFoundException>(() => _sut.Handle(new AddCreatorFollowCommand(missingId), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenUserFollowsThemselves_WhenCalled_ThenThrowsCannotFollowSelfException()
    {
        _memberRepository.GetByIdAsync(_userId, Arg.Any<CancellationToken>()).Returns(Member.Create("Self"));

        Assert.ThrowsAsync<CannotFollowSelfException>(() => _sut.Handle(new AddCreatorFollowCommand(_userId), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenUnauthenticatedUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() => _sut.Handle(new AddCreatorFollowCommand(_creator.Id), CancellationToken.None));
    }
}
