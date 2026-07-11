using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Application.Identity.Dtos;
using ChoicePie.Backend.Application.Identity.Queries;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Identity;

[TestFixture]
public class GetCurrentMemberQueryHandlerTests
{
    private IMemberQueryService _memberQueryService = null!;
    private ICurrentUserService _currentUserService = null!;
    private GetCurrentMemberQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _memberQueryService = Substitute.For<IMemberQueryService>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _sut = new GetCurrentMemberQueryHandler(_memberQueryService, _currentUserService);
    }

    [Test]
    public async Task Handle_GivenAuthenticatedMember_WhenCalled_ThenReturnsMemberQueryServiceResult()
    {
        var memberId = Guid.NewGuid();
        var dto = new MemberDto(memberId, "host@example.com", "Host Name", null, false, DateTime.UtcNow);
        _currentUserService.UserId.Returns(memberId);
        _memberQueryService.GetByIdAsync(memberId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await _sut.Handle(new GetCurrentMemberQuery(), CancellationToken.None);

        Assert.That(result, Is.SameAs(dto));
    }

    [Test]
    public void Handle_GivenNoCurrentUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() => _sut.Handle(new GetCurrentMemberQuery(), CancellationToken.None));
    }
}
