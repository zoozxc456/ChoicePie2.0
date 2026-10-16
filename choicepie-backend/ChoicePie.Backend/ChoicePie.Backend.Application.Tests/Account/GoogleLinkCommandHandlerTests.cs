using ChoicePie.Backend.Application.Account.Commands;
using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Account;

[TestFixture]
public class GoogleLinkCommandHandlerTests
{
    private static readonly Guid MemberId = Guid.NewGuid();
    private static readonly GoogleIdentity Identity = new("google-sub", "g@example.com", "G", null);

    private IGoogleIdTokenVerifier _verifier = null!;
    private IAuthAccountRepository _authAccountRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private AuthAccount _account = null!;

    [SetUp]
    public void SetUp()
    {
        _verifier = Substitute.For<IGoogleIdTokenVerifier>();
        _authAccountRepository = Substitute.For<IAuthAccountRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentUserService.UserId.Returns(MemberId);
        _verifier.VerifyAsync("token", Arg.Any<CancellationToken>()).Returns(Identity);

        _account = AuthAccount.Register(Email.Create("a@example.com"), HashedPassword.Create("h", "s"), MemberId);
        _authAccountRepository
            .FirstOrDefaultAsync(Arg.Any<AuthAccountByMemberIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(_account);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    private LinkGoogleCommandHandler LinkSut() =>
        new(_verifier, _authAccountRepository, _currentUserService, _unitOfWork);

    [Test]
    public async Task Link_GivenUnusedGoogleIdentity_WhenCalled_ThenLinksGoogleToCurrentAccount()
    {
        await LinkSut().Handle(new LinkGoogleCommand { IdToken = "token" }, CancellationToken.None);

        Assert.That(_account.IsLinkedTo(LoginProvider.Google), Is.True);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Link_GivenGoogleIdentityLinkedToAnotherAccount_WhenCalled_ThenThrowsExternalIdentityAlreadyLinkedException()
    {
        var other = AuthAccount.RegisterWithExternalLogin(
            Email.Create("other@example.com"), LoginProvider.Google, "google-sub", Guid.NewGuid());
        _authAccountRepository
            .FirstOrDefaultAsync(Arg.Any<AuthAccountByExternalIdentitySpecification>(), Arg.Any<CancellationToken>())
            .Returns(other);

        Assert.ThrowsAsync<ExternalIdentityAlreadyLinkedException>(() =>
            LinkSut().Handle(new LinkGoogleCommand { IdToken = "token" }, CancellationToken.None));
        Assert.That(_account.IsLinkedTo(LoginProvider.Google), Is.False);
    }

    [Test]
    public void Link_GivenAccountAlreadyLinkedToGoogle_WhenCalled_ThenThrowsLoginMethodAlreadyLinkedException()
    {
        _account.AddLoginMethod(LoginProvider.Google, "google-sub");
        _authAccountRepository
            .FirstOrDefaultAsync(Arg.Any<AuthAccountByExternalIdentitySpecification>(), Arg.Any<CancellationToken>())
            .Returns(_account);

        Assert.ThrowsAsync<LoginMethodAlreadyLinkedException>(() =>
            LinkSut().Handle(new LinkGoogleCommand { IdToken = "token" }, CancellationToken.None));
    }

    [Test]
    public async Task Unlink_GivenGoogleAndPassword_WhenCalled_ThenRemovesGoogle()
    {
        _account.AddLoginMethod(LoginProvider.Google, "google-sub");
        var sut = new UnlinkGoogleCommandHandler(_authAccountRepository, _currentUserService, _unitOfWork);

        await sut.Handle(new UnlinkGoogleCommand(), CancellationToken.None);

        Assert.That(_account.IsLinkedTo(LoginProvider.Google), Is.False);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Unlink_GivenGoogleIsLastMethod_WhenCalled_ThenThrowsCannotRemoveLastLoginMethodException()
    {
        var googleOnly = AuthAccount.RegisterWithExternalLogin(
            Email.Create("a@example.com"), LoginProvider.Google, "google-sub", MemberId);
        _authAccountRepository
            .FirstOrDefaultAsync(Arg.Any<AuthAccountByMemberIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(googleOnly);
        var sut = new UnlinkGoogleCommandHandler(_authAccountRepository, _currentUserService, _unitOfWork);

        Assert.ThrowsAsync<CannotRemoveLastLoginMethodException>(() =>
            sut.Handle(new UnlinkGoogleCommand(), CancellationToken.None));
    }
}
