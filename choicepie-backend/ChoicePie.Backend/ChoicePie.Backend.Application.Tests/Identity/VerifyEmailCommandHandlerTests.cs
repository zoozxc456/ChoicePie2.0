using ChoicePie.Backend.Application.Identity.Commands;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken;
using ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;
using AuthAccountAggregate = ChoicePie.Backend.Domain.Aggregates.AuthAccount.AuthAccount;
using EmailVerificationTokenAggregate = ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.EmailVerificationToken;

namespace ChoicePie.Backend.Application.Tests.Identity;

[TestFixture]
public class VerifyEmailCommandHandlerTests
{
    private IEmailVerificationTokenRepository _emailVerificationTokenRepository = null!;
    private IAuthAccountRepository _authAccountRepository = null!;
    private IRefreshTokenGenerator _tokenGenerator = null!;
    private IUnitOfWork _unitOfWork = null!;
    private TimeProvider _timeProvider = null!;
    private VerifyEmailCommandHandler _sut = null!;
    private AuthAccountAggregate _authAccount = null!;
    private EmailVerificationTokenAggregate _verificationToken = null!;
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    [SetUp]
    public void SetUp()
    {
        _emailVerificationTokenRepository = Substitute.For<IEmailVerificationTokenRepository>();
        _authAccountRepository = Substitute.For<IAuthAccountRepository>();
        _tokenGenerator = Substitute.For<IRefreshTokenGenerator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(_now);
        _sut = new VerifyEmailCommandHandler(
            _emailVerificationTokenRepository, _authAccountRepository, _tokenGenerator, _unitOfWork, _timeProvider);

        _authAccount = AuthAccountAggregate.Register(
            Email.Create("user@example.com"), HashedPassword.Create("hash", "salt"), Guid.NewGuid());
        _verificationToken = EmailVerificationTokenAggregate.Issue(
            _authAccount.Id, "user@example.com", "raw-token", "token-hash", _now.UtcDateTime);

        _tokenGenerator.Hash("raw-token").Returns("token-hash");
        _emailVerificationTokenRepository.FirstOrDefaultAsync(Arg.Any<EmailVerificationTokenByTokenHashSpecification>(), Arg.Any<CancellationToken>())
            .Returns(_verificationToken);
        _authAccountRepository.GetByIdAsync(_authAccount.Id, Arg.Any<CancellationToken>()).Returns(_authAccount);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenValidToken_WhenCalled_ThenVerifiesAccountAndMarksTokenUsed()
    {
        await _sut.Handle(new VerifyEmailCommand { Token = "raw-token" }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_authAccount.IsVerified, Is.True);
            Assert.That(_verificationToken.IsActive, Is.False);
        });
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenUnknownToken_WhenCalled_ThenThrowsInvalidEmailVerificationTokenException()
    {
        _emailVerificationTokenRepository.FirstOrDefaultAsync(Arg.Any<EmailVerificationTokenByTokenHashSpecification>(), Arg.Any<CancellationToken>())
            .Returns((EmailVerificationTokenAggregate?)null);

        Assert.ThrowsAsync<InvalidEmailVerificationTokenException>(() =>
            _sut.Handle(new VerifyEmailCommand { Token = "raw-token" }, CancellationToken.None));
    }

    [Test]
    public void Handle_GivenExpiredToken_WhenCalled_ThenThrowsInvalidEmailVerificationTokenException()
    {
        _timeProvider.GetUtcNow().Returns(_now.AddHours(25));

        Assert.ThrowsAsync<InvalidEmailVerificationTokenException>(() =>
            _sut.Handle(new VerifyEmailCommand { Token = "raw-token" }, CancellationToken.None));
    }
}
