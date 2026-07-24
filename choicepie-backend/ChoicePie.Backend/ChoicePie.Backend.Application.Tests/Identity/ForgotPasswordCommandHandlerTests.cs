using ChoicePie.Backend.Application.Identity.Commands;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.PasswordResetToken;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;
using AuthAccountAggregate = ChoicePie.Backend.Domain.Aggregates.AuthAccount.AuthAccount;

namespace ChoicePie.Backend.Application.Tests.Identity;

[TestFixture]
public class ForgotPasswordCommandHandlerTests
{
    private IAuthAccountRepository _authAccountRepository = null!;
    private IPasswordResetTokenRepository _passwordResetTokenRepository = null!;
    private IRefreshTokenGenerator _tokenGenerator = null!;
    private IUnitOfWork _unitOfWork = null!;
    private TimeProvider _timeProvider = null!;
    private ForgotPasswordCommandHandler _sut = null!;
    private AuthAccountAggregate _authAccount = null!;

    [SetUp]
    public void SetUp()
    {
        _authAccountRepository = Substitute.For<IAuthAccountRepository>();
        _passwordResetTokenRepository = Substitute.For<IPasswordResetTokenRepository>();
        _tokenGenerator = Substitute.For<IRefreshTokenGenerator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);
        _sut = new ForgotPasswordCommandHandler(
            _authAccountRepository, _passwordResetTokenRepository, _tokenGenerator, _unitOfWork, _timeProvider);

        _authAccount = AuthAccountAggregate.Register(
            Email.Create("user@example.com"), HashedPassword.Create("hash", "salt"), Guid.NewGuid());
        _authAccountRepository.FirstOrDefaultAsync(Arg.Any<AuthAccountByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(_authAccount);
        _tokenGenerator.Generate().Returns(("raw-token", "token-hash"));
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenExistingEmail_WhenCalled_ThenIssuesTokenAndPersists()
    {
        await _sut.Handle(new ForgotPasswordCommand { Email = "user@example.com" }, CancellationToken.None);

        await _passwordResetTokenRepository.Received(1).AddAsync(
            Arg.Is<ChoicePie.Backend.Domain.Aggregates.PasswordResetToken.PasswordResetToken>(t => t.AuthAccountId == _authAccount.Id),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenUnknownEmail_WhenCalled_ThenSilentlyNoOps()
    {
        _authAccountRepository.FirstOrDefaultAsync(Arg.Any<AuthAccountByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns((AuthAccountAggregate?)null);

        await _sut.Handle(new ForgotPasswordCommand { Email = "missing@example.com" }, CancellationToken.None);

        await _passwordResetTokenRepository.DidNotReceive().AddAsync(
            Arg.Any<ChoicePie.Backend.Domain.Aggregates.PasswordResetToken.PasswordResetToken>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
