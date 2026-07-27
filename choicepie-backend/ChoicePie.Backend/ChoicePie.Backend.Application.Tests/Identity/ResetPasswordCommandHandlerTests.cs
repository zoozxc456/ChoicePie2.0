using ChoicePie.Backend.Application.Identity.Commands;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.PasswordResetToken;
using ChoicePie.Backend.Domain.Aggregates.PasswordResetToken.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.PasswordResetToken.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;
using AuthAccountAggregate = ChoicePie.Backend.Domain.Aggregates.AuthAccount.AuthAccount;
using PasswordResetTokenAggregate = ChoicePie.Backend.Domain.Aggregates.PasswordResetToken.PasswordResetToken;

namespace ChoicePie.Backend.Application.Tests.Identity;

[TestFixture]
public class ResetPasswordCommandHandlerTests
{
    private IPasswordResetTokenRepository _passwordResetTokenRepository = null!;
    private IAuthAccountRepository _authAccountRepository = null!;
    private IRefreshTokenGenerator _tokenGenerator = null!;
    private IPasswordHasher _passwordHasher = null!;
    private IUnitOfWork _unitOfWork = null!;
    private TimeProvider _timeProvider = null!;
    private ResetPasswordCommandHandler _sut = null!;
    private AuthAccountAggregate _authAccount = null!;
    private PasswordResetTokenAggregate _resetToken = null!;
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    [SetUp]
    public void SetUp()
    {
        _passwordResetTokenRepository = Substitute.For<IPasswordResetTokenRepository>();
        _authAccountRepository = Substitute.For<IAuthAccountRepository>();
        _tokenGenerator = Substitute.For<IRefreshTokenGenerator>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(_now);
        _sut = new ResetPasswordCommandHandler(
            _passwordResetTokenRepository, _authAccountRepository, _tokenGenerator, _passwordHasher, _unitOfWork, _timeProvider);

        _authAccount = AuthAccountAggregate.Register(
            Email.Create("user@example.com"), HashedPassword.Create("hash", "salt"), Guid.NewGuid());
        _resetToken = PasswordResetTokenAggregate.Issue(
            _authAccount.Id, "user@example.com", "raw-token", "token-hash", _now.UtcDateTime);

        _tokenGenerator.Hash("raw-token").Returns("token-hash");
        _passwordResetTokenRepository.FirstOrDefaultAsync(Arg.Any<PasswordResetTokenByTokenHashSpecification>(), Arg.Any<CancellationToken>())
            .Returns(_resetToken);
        _authAccountRepository.GetByIdAsync(_authAccount.Id, Arg.Any<CancellationToken>()).Returns(_authAccount);
        _passwordHasher.Hash(Arg.Any<string>()).Returns(HashedPassword.Create("new-hash", "new-salt"));
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    private static ResetPasswordCommand ValidCommand() => new()
    {
        Token = "raw-token", Password = "newpassword1", ConfirmPassword = "newpassword1"
    };

    [Test]
    public async Task Handle_GivenValidToken_WhenCalled_ThenChangesPasswordAndMarksTokenUsed()
    {
        await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_authAccount.OriginalPassword, Is.EqualTo(HashedPassword.Create("new-hash", "new-salt")));
            Assert.That(_resetToken.IsActive, Is.False);
        });
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenUnknownToken_WhenCalled_ThenThrowsInvalidPasswordResetTokenException()
    {
        _passwordResetTokenRepository.FirstOrDefaultAsync(Arg.Any<PasswordResetTokenByTokenHashSpecification>(), Arg.Any<CancellationToken>())
            .Returns((PasswordResetTokenAggregate?)null);

        Assert.ThrowsAsync<InvalidPasswordResetTokenException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenExpiredToken_WhenCalled_ThenThrowsInvalidPasswordResetTokenException()
    {
        _timeProvider.GetUtcNow().Returns(_now.AddMinutes(31));

        Assert.ThrowsAsync<InvalidPasswordResetTokenException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenAlreadyUsedToken_WhenCalled_ThenThrowsInvalidPasswordResetTokenException()
    {
        _resetToken.MarkUsed(_now.UtcDateTime);

        Assert.ThrowsAsync<InvalidPasswordResetTokenException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }
}
