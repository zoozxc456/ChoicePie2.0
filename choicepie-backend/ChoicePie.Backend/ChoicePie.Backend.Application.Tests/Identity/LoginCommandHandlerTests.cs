using ChoicePie.Backend.Application.Identity.Commands;
using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member.Specifications;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Identity;

[TestFixture]
public class LoginCommandHandlerTests
{
    private IMemberRepository _memberRepository = null!;
    private IPasswordHasher _passwordHasher = null!;
    private ITokenService _tokenService = null!;
    private LoginCommandHandler _sut = null!;
    private Member _registeredMember = null!;

    [SetUp]
    public void SetUp()
    {
        _memberRepository = Substitute.For<IMemberRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _tokenService = Substitute.For<ITokenService>();
        _sut = new LoginCommandHandler(_memberRepository, _passwordHasher, _tokenService);

        _registeredMember = Member.Register(Email.Create("host@example.com"), "Host Name", "hashed-password");
        _memberRepository.FirstOrDefaultAsync(Arg.Any<MemberByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(_registeredMember);
    }

    private static LoginCommand ValidCommand() => new() { Email = "host@example.com", Password = "correct-password" };

    [Test]
    public async Task Handle_GivenValidCredentials_WhenCalled_ThenReturnsMemberAndToken()
    {
        _passwordHasher.Verify("correct-password", "hashed-password").Returns(true);
        _tokenService.GenerateToken(_registeredMember).Returns("jwt-token");

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Token, Is.EqualTo("jwt-token"));
            Assert.That(result.Member.Email, Is.EqualTo("host@example.com"));
        });
    }

    [Test]
    public void Handle_GivenUnknownEmail_WhenCalled_ThenThrowsInvalidCredentialsException()
    {
        _memberRepository.FirstOrDefaultAsync(Arg.Any<MemberByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Member?)null);

        Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenWrongPassword_WhenCalled_ThenThrowsInvalidCredentialsException()
    {
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }
}
