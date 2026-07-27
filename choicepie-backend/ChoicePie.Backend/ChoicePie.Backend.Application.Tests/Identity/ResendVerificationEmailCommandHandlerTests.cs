using ChoicePie.Backend.Application.Identity.Commands;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;
using AuthAccountAggregate = ChoicePie.Backend.Domain.Aggregates.AuthAccount.AuthAccount;

namespace ChoicePie.Backend.Application.Tests.Identity;

[TestFixture]
public class ResendVerificationEmailCommandHandlerTests
{
    private IAuthAccountRepository _authAccountRepository = null!;
    private IEmailVerificationTokenRepository _emailVerificationTokenRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private IRefreshTokenGenerator _tokenGenerator = null!;
    private IUnitOfWork _unitOfWork = null!;
    private TimeProvider _timeProvider = null!;
    private ResendVerificationEmailCommandHandler _sut = null!;
    private readonly Guid _memberId = Guid.NewGuid();
    private AuthAccountAggregate _authAccount = null!;

    [SetUp]
    public void SetUp()
    {
        _authAccountRepository = Substitute.For<IAuthAccountRepository>();
        _emailVerificationTokenRepository = Substitute.For<IEmailVerificationTokenRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _tokenGenerator = Substitute.For<IRefreshTokenGenerator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);
        _sut = new ResendVerificationEmailCommandHandler(
            _authAccountRepository, _emailVerificationTokenRepository, _currentUserService, _tokenGenerator, _unitOfWork, _timeProvider);

        _authAccount = AuthAccountAggregate.Register(
            Email.Create("user@example.com"), HashedPassword.Create("hash", "salt"), _memberId);
        _currentUserService.UserId.Returns(_memberId);
        _authAccountRepository.FirstOrDefaultAsync(Arg.Any<AuthAccountByMemberIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(_authAccount);
        _tokenGenerator.Generate().Returns(("raw-token", "token-hash"));
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenUnverifiedAccount_WhenCalled_ThenIssuesTokenAndPersists()
    {
        await _sut.Handle(new ResendVerificationEmailCommand(), CancellationToken.None);

        await _emailVerificationTokenRepository.Received(1).AddAsync(
            Arg.Any<ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.EmailVerificationToken>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenAlreadyVerifiedAccount_WhenCalled_ThenThrowsEmailAlreadyVerifiedException()
    {
        _authAccount.Verify();

        Assert.ThrowsAsync<EmailAlreadyVerifiedException>(() =>
            _sut.Handle(new ResendVerificationEmailCommand(), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenUnauthenticatedUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() =>
            _sut.Handle(new ResendVerificationEmailCommand(), CancellationToken.None));
    }
}
