using ChoicePie.Backend.Application.Identity.Commands;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;
using ChoicePie.Backend.Shared.Kernel.Exceptions;
using NSubstitute.ExceptionExtensions;

namespace ChoicePie.Backend.Application.Tests.Identity;

[TestFixture]
public class RegisterMemberCommandHandlerTests
{
    private IMemberRepository _memberRepository = null!;
    private IAuthAccountRepository _authAccountRepository = null!;
    private IEmailVerificationTokenRepository _emailVerificationTokenRepository = null!;
    private IMembershipTierRepository _membershipTierRepository = null!;
    private IUnitOfWork _unitOfWork = null!;
    private IPasswordHasher _passwordHasher = null!;
    private IRefreshTokenGenerator _tokenGenerator = null!;
    private TimeProvider _timeProvider = null!;
    private RegisterMemberCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _memberRepository = Substitute.For<IMemberRepository>();
        _authAccountRepository = Substitute.For<IAuthAccountRepository>();
        _emailVerificationTokenRepository = Substitute.For<IEmailVerificationTokenRepository>();
        _membershipTierRepository = Substitute.For<IMembershipTierRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _tokenGenerator = Substitute.For<IRefreshTokenGenerator>();
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);
        _sut = new RegisterMemberCommandHandler(_memberRepository, _authAccountRepository,
            _emailVerificationTokenRepository, _membershipTierRepository, _passwordHasher, _tokenGenerator, _unitOfWork, _timeProvider);

        _authAccountRepository.ExistsAsync(Arg.Any<AuthAccountByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.Hash(Arg.Any<string>()).Returns(HashedPassword.Create("hashed-password", "salt"));
        _tokenGenerator.Generate().Returns(("raw-token", "token-hash"));
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    private static RegisterMemberCommand ValidCommand() => new()
    {
        Email = "host@example.com",
        Name = "Host Name",
        Password = "password123",
        ConfirmPassword = "password123"
    };

    [Test]
    public async Task Handle_GivenDefaultTierExists_WhenCalled_ThenAssignsDefaultTierToNewMember()
    {
        var defaultTier = MembershipTier.Create("Free", 3, 10_000, isDefault: true);
        _membershipTierRepository
            .FirstOrDefaultAsync(Arg.Any<DefaultMembershipTierSpecification>(), Arg.Any<CancellationToken>())
            .Returns(defaultTier);

        await _sut.Handle(ValidCommand(), CancellationToken.None);

        await _memberRepository.Received(1).AddAsync(
            Arg.Is<Member>(m => m.TierId == defaultTier.Id), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenNoDefaultTier_WhenCalled_ThenRegistersMemberWithoutTier()
    {
        _membershipTierRepository
            .FirstOrDefaultAsync(Arg.Any<DefaultMembershipTierSpecification>(), Arg.Any<CancellationToken>())
            .Returns((MembershipTier?)null);

        await _sut.Handle(ValidCommand(), CancellationToken.None);

        await _memberRepository.Received(1).AddAsync(
            Arg.Is<Member>(m => m.TierId == null), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenNewEmail_WhenCalled_ThenPersistsMemberAndAuthAccount()
    {
        await _sut.Handle(ValidCommand(), CancellationToken.None);

        await _memberRepository.Received(1).AddAsync(
            Arg.Is<Member>(m => m.Name == "Host Name"),
            Arg.Any<CancellationToken>());
        await _authAccountRepository.Received(1).AddAsync(
            Arg.Is<AuthAccount>(a =>
                a.Email.Value == "host@example.com" &&
                a.OriginalPassword == HashedPassword.Create("hashed-password", "salt")),
            Arg.Any<CancellationToken>());
        await _emailVerificationTokenRepository.Received(1).AddAsync(
            Arg.Any<EmailVerificationToken>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenNewEmail_WhenCalled_ThenAuthAccountLinksToPersistedMember()
    {
        Member? capturedMember = null;
        AuthAccount? capturedAuthAccount = null;
        _ = _memberRepository.AddAsync(Arg.Do<Member>(m => capturedMember = m), Arg.Any<CancellationToken>());
        _ = _authAccountRepository.AddAsync(Arg.Do<AuthAccount>(a => capturedAuthAccount = a),
            Arg.Any<CancellationToken>());

        await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.That(capturedAuthAccount!.MemberId, Is.EqualTo(capturedMember!.Id));
    }

    [Test]
    public async Task Handle_GivenNewEmail_WhenCalled_ThenReturnsMemberDto()
    {
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Email, Is.EqualTo("host@example.com"));
            Assert.That(result.Name, Is.EqualTo("Host Name"));
            Assert.That(result.IsVerified, Is.False);
        });
    }

    [Test]
    public void Handle_GivenEmailAlreadyRegistered_WhenCalled_ThenThrowsEmailAlreadyRegisteredException()
    {
        _authAccountRepository.ExistsAsync(Arg.Any<AuthAccountByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        Assert.ThrowsAsync<EmailAlreadyRegisteredException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public async Task Handle_GivenEmailAlreadyRegistered_WhenCalled_ThenDoesNotPersistMemberOrAuthAccount()
    {
        _authAccountRepository.ExistsAsync(Arg.Any<AuthAccountByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        try
        {
            await _sut.Handle(ValidCommand(), CancellationToken.None);
        }
        catch (EmailAlreadyRegisteredException)
        {
            // expected
        }

        await _memberRepository.DidNotReceive().AddAsync(Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _authAccountRepository.DidNotReceive().AddAsync(Arg.Any<AuthAccount>(), Arg.Any<CancellationToken>());
    }

    // 同一個 Email 同時註冊兩次：兩邊都通過了「是否已註冊」的檢查，由唯一索引擋下後要回「已註冊」。
    [Test]
    public void Handle_GivenSameEmailRegisteredConcurrently_WhenSaving_ThenThrowsEmailAlreadyRegistered()
    {
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new UniqueConstraintViolationException("ix"));

        Assert.ThrowsAsync<EmailAlreadyRegisteredException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }
}
