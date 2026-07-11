using ChoicePie.Backend.Application.Identity.Commands;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Identity;

[TestFixture]
public class RegisterMemberCommandHandlerTests
{
    private IMemberRepository _memberRepository = null!;
    private IAuthAccountRepository _authAccountRepository = null!;
    private IUnitOfWork _unitOfWork = null!;
    private IPasswordHasher _passwordHasher = null!;
    private RegisterMemberCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _memberRepository = Substitute.For<IMemberRepository>();
        _authAccountRepository = Substitute.For<IAuthAccountRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _sut = new RegisterMemberCommandHandler(_memberRepository, _authAccountRepository, _passwordHasher,
            _unitOfWork);

        _authAccountRepository.ExistsAsync(Arg.Any<AuthAccountByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.Hash(Arg.Any<string>()).Returns(HashedPassword.Create("hashed-password", "salt"));
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
}