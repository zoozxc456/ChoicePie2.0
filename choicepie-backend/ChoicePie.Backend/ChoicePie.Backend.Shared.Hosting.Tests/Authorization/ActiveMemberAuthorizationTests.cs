using System.Security.Claims;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Hosting.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace ChoicePie.Backend.Shared.Hosting.Tests.Authorization;

[TestFixture]
public class ActiveMemberAuthorizationTests
{
    private IMemberActivityChecker _checker = null!;
    private ActiveMemberAuthorizationHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _checker = Substitute.For<IMemberActivityChecker>();
        _sut = new ActiveMemberAuthorizationHandler(_checker);
    }

    private static AuthorizationHandlerContext BuildContext(params Claim[] claims) =>
        new([new ActiveMemberRequirement()], new ClaimsPrincipal(new ClaimsIdentity(claims, "test")), null);

    [Test]
    public async Task HandleAsync_GivenActiveMember_WhenEvaluated_ThenSucceeds()
    {
        var id = Guid.NewGuid();
        _checker.IsActiveAsync(id, Arg.Any<CancellationToken>()).Returns(true);
        var context = BuildContext(new Claim("sub", id.ToString()));

        await _sut.HandleAsync(context);

        Assert.That(context.HasSucceeded, Is.True);
    }

    [Test]
    public async Task HandleAsync_GivenDeletedOrSuspendedMember_WhenEvaluated_ThenFails()
    {
        var id = Guid.NewGuid();
        _checker.IsActiveAsync(id, Arg.Any<CancellationToken>()).Returns(false);
        var context = BuildContext(new Claim("sub", id.ToString()));

        await _sut.HandleAsync(context);

        Assert.Multiple(() =>
        {
            Assert.That(context.HasSucceeded, Is.False);
            Assert.That(context.HasFailed, Is.True);
        });
    }

    [Test]
    public async Task HandleAsync_GivenSuspensionExpiredSoCheckerReportsActive_WhenEvaluated_ThenSucceeds()
    {
        var id = Guid.NewGuid();
        _checker.IsActiveAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        var context = BuildContext(new Claim("sub", id.ToString()));
        await _sut.HandleAsync(context);

        Assert.That(context.HasSucceeded, Is.True);
    }

    [TestCase(null)]
    [TestCase("not-a-guid")]
    public async Task HandleAsync_GivenMissingOrInvalidSub_WhenEvaluated_ThenFailsWithoutCallingChecker(string? sub)
    {
        var context = sub is null ? BuildContext() : BuildContext(new Claim("sub", sub));

        await _sut.HandleAsync(context);

        Assert.That(context.HasFailed, Is.True);
        await _checker.DidNotReceive().IsActiveAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ResultHandler_GivenInactiveMemberFailure_WhenHandled_ThenRespondsWith401AndSkipsNext()
    {
        var id = Guid.NewGuid();
        _checker.IsActiveAsync(id, Arg.Any<CancellationToken>()).Returns(false);
        var authContext = BuildContext(new Claim("sub", id.ToString()));
        await _sut.HandleAsync(authContext);
        var failure = AuthorizationFailure.Failed(authContext.FailureReasons);
        var httpContext = new DefaultHttpContext();
        var nextCalled = false;
        var sut = new ActiveMemberAuthorizationResultHandler();

        await sut.HandleAsync(_ => { nextCalled = true; return Task.CompletedTask; }, httpContext,
            new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build(),
            PolicyAuthorizationResult.Forbid(failure));

        Assert.Multiple(() =>
        {
            Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
            Assert.That(nextCalled, Is.False);
        });
    }
}
