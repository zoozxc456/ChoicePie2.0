using ChoicePie.Backend.Application.Identity.Commands;
using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member.Specifications;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Identity;

[TestFixture]
public class RegisterMemberCommandHandlerTests
{
    private IMemberRepository _memberRepository = null!;
    private IUnitOfWork _unitOfWork = null!;
    private IPasswordHasher _passwordHasher = null!;
    private RegisterMemberCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _memberRepository = Substitute.For<IMemberRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _sut = new RegisterMemberCommandHandler(_memberRepository, _passwordHasher, _unitOfWork);
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
    public async Task Handle_GivenNewEmail_WhenCalled_ThenHashesPasswordAndPersistsMember()
    {
        _memberRepository.ExistsAsync(Arg.Any<MemberByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.Hash("password123").Returns("hashed-password");

        await _sut.Handle(ValidCommand(), CancellationToken.None);

        await _memberRepository.Received(1).AddAsync(
            Arg.Is<Member>(m => m.Email.Value == "host@example.com" && m.PasswordHash == "hashed-password"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenNewEmail_WhenCalled_ThenReturnsMemberDto()
    {
        _memberRepository.ExistsAsync(Arg.Any<MemberByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed-password");

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
        _memberRepository.ExistsAsync(Arg.Any<MemberByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        Assert.ThrowsAsync<EmailAlreadyRegisteredException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public async Task Handle_GivenEmailAlreadyRegistered_WhenCalled_ThenDoesNotPersistMember()
    {
        _memberRepository.ExistsAsync(Arg.Any<MemberByEmailSpecification>(), Arg.Any<CancellationToken>())
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
    }
}
