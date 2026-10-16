using ChoicePie.Backend.Application.CreatorFollows.Contracts;
using ChoicePie.Backend.Application.CreatorFollows.Dtos;
using ChoicePie.Backend.Application.CreatorFollows.Queries;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Application.Interfaces;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.CreatorFollows;

[TestFixture]
public class ListMyFollowingQueryHandlerTests
{
    private ICreatorQueryService _creatorQueryService = null!;
    private ICurrentUserService _currentUserService = null!;
    private ListMyFollowingQueryHandler _sut = null!;
    private readonly Guid _userId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _creatorQueryService = Substitute.For<ICreatorQueryService>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _sut = new ListMyFollowingQueryHandler(_creatorQueryService, _currentUserService);
    }

    [Test]
    public async Task Handle_GivenAuthenticatedUser_WhenCalled_ThenReturnsFollowedCreators()
    {
        _currentUserService.UserId.Returns(_userId);
        var expected = new PagedResult<FollowedCreatorDto>([], 1, 20, 0);
        _creatorQueryService.ListFollowingAsync(_userId, 1, 20, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.Handle(new ListMyFollowingQuery(1, 20), CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
    }

    [Test]
    public void Handle_GivenNoCurrentUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() =>
            _sut.Handle(new ListMyFollowingQuery(1, 20), CancellationToken.None));
    }
}
