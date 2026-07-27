using System.IdentityModel.Tokens.Jwt;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Infrastructure.Identity;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using ChoicePie.Backend.Shared.Kernel.Auth;
using Microsoft.Extensions.Options;

namespace ChoicePie.Backend.Infrastructure.Tests.Identity;

[TestFixture]
public class JwtTokenServiceTests
{
    private JwtTokenService _sut = null!;
    private JwtSettings _settings = null!;

    [SetUp]
    public void SetUp()
    {
        _settings = new JwtSettings
        {
            SigningKey = "unit-test-signing-key-at-least-32-bytes-long!!",
            Issuer = "ChoicePie.Tests",
            Audience = "ChoicePie.Tests.Audience",
            AccessTokenExpirationSeconds = 120
        };
        _sut = new JwtTokenService(Options.Create(_settings));
    }

    [Test]
    public void GenerateAccessToken_GivenMember_WhenCalled_ThenTokenContainsMemberSubjectAndMemberRoleClaim()
    {
        var member = Member.Create("Test Member");

        var token = _sut.GenerateAccessToken(member);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.That(jwt.Subject, Is.EqualTo(member.Id.ToString()));
        Assert.That(jwt.Claims.Single(c => c.Type == JwtClaimValues.RoleClaimType).Value, Is.EqualTo(JwtClaimValues.MemberRole));
        Assert.That(jwt.Issuer, Is.EqualTo(_settings.Issuer));
        Assert.That(jwt.Audiences, Does.Contain(_settings.Audience));
    }

    [Test]
    public void GenerateAccessToken_GivenMember_WhenCalled_ThenExpiryMatchesConfiguredLifetime()
    {
        var member = Member.Create("Test Member");
        var before = DateTime.UtcNow;

        var token = _sut.GenerateAccessToken(member);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var expectedExpiry = before.AddSeconds(_settings.AccessTokenExpirationSeconds);
        Assert.That(jwt.ValidTo, Is.EqualTo(expectedExpiry).Within(TimeSpan.FromSeconds(5)));
    }

    [Test]
    public void GenerateAccessToken_GivenDifferentMembers_WhenCalled_ThenTokensHaveDifferentSubjects()
    {
        var memberA = Member.Create("Member A");
        var memberB = Member.Create("Member B");

        var tokenA = new JwtSecurityTokenHandler().ReadJwtToken(_sut.GenerateAccessToken(memberA));
        var tokenB = new JwtSecurityTokenHandler().ReadJwtToken(_sut.GenerateAccessToken(memberB));

        Assert.That(tokenA.Subject, Is.Not.EqualTo(tokenB.Subject));
    }
}
