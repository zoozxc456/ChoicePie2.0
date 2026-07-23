using ChoicePie.Backend.Application.CreatorFollows.Contracts;
using ChoicePie.Backend.Application.CreatorFollows.Dtos;
using ChoicePie.Backend.Application.CreatorFollows.Queries;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.CreatorFollows;

[TestFixture]
public class GetCreatorProfileQueryHandlerTests
{
    private ICreatorQueryService _creatorQueryService = null!;
    private ICurrentUserService _currentUserService = null!;
    private GetCreatorProfileQueryHandler _sut = null!;
    private readonly Guid _creatorId = Guid.NewGuid();
    private readonly Guid _currentUserId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _creatorQueryService = Substitute.For<ICreatorQueryService>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _sut = new GetCreatorProfileQueryHandler(_creatorQueryService, _currentUserService);
    }

    [Test]
    public async Task Handle_GivenExistingCreator_WhenCalled_ThenReturnsProfileUsingCurrentUserId()
    {
        _currentUserService.UserId.Returns(_currentUserId);
        var expected = new CreatorProfileDto(_creatorId, "Alice", null, 3, 10, true);
        _creatorQueryService.GetByIdAsync(_creatorId, _currentUserId, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.Handle(new GetCreatorProfileQuery(_creatorId), CancellationToken.None);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public async Task Handle_GivenAnonymousUser_WhenCalled_ThenPassesNullCurrentUserId()
    {
        _currentUserService.UserId.Returns((Guid?)null);
        var expected = new CreatorProfileDto(_creatorId, "Alice", null, 3, 10, false);
        _creatorQueryService.GetByIdAsync(_creatorId, null, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.Handle(new GetCreatorProfileQuery(_creatorId), CancellationToken.None);

        Assert.That(result.IsFollowing, Is.False);
    }

    [Test]
    public void Handle_GivenCreatorNotFound_WhenCalled_ThenThrowsMemberNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _creatorQueryService.GetByIdAsync(missingId, Arg.Any<Guid?>(), Arg.Any<CancellationToken>()).Returns((CreatorProfileDto?)null);

        Assert.ThrowsAsync<MemberNotFoundException>(() => _sut.Handle(new GetCreatorProfileQuery(missingId), CancellationToken.None));
    }
}
