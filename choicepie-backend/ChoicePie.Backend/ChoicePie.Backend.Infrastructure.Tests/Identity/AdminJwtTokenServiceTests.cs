using System.IdentityModel.Tokens.Jwt;
using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Enums;
using ChoicePie.Backend.Infrastructure.Identity;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using ChoicePie.Backend.Shared.Kernel.Auth;
using Microsoft.Extensions.Options;

namespace ChoicePie.Backend.Infrastructure.Tests.Identity;

[TestFixture]
public class AdminJwtTokenServiceTests
{
    private AdminJwtTokenService _sut = null!;
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
        _sut = new AdminJwtTokenService(Options.Create(_settings));
    }

    [Test]
    public void GenerateAccessToken_GivenAdminUser_WhenCalled_ThenTokenContainsAdminSubjectAndAdminRoleClaim()
    {
        var adminUser = AdminUser.Create("Test Admin", AdminRole.Admin);

        var token = _sut.GenerateAccessToken(adminUser);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.That(jwt.Subject, Is.EqualTo(adminUser.Id.ToString()));
        Assert.That(jwt.Claims.Single(c => c.Type == JwtClaimValues.RoleClaimType).Value, Is.EqualTo(JwtClaimValues.AdminRole));
    }

    [Test]
    public void GenerateAccessToken_GivenAdminUser_WhenCalled_ThenRoleClaimIsAdminRegardlessOfAdminUserRoleEnum()
    {
        // JwtClaimValues.AdminRole is the coarse-grained "this is an admin-side account" claim used by
        // [Authorize(Policy="AdminOnly")]; the fine-grained AdminUser.Role (staff/maintainer/systemAdmin)
        // is a separate authorization concern not encoded into the JWT role claim.
        var maintainer = AdminUser.Create("Maintainer", AdminRole.Maintainer);

        var token = _sut.GenerateAccessToken(maintainer);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.That(jwt.Claims.Single(c => c.Type == JwtClaimValues.RoleClaimType).Value, Is.EqualTo(JwtClaimValues.AdminRole));
    }

    [Test]
    public void GenerateAccessToken_GivenAdminUser_WhenCalled_ThenIssuerAndAudienceMatchConfiguredSettings()
    {
        var adminUser = AdminUser.Create("Test Admin", AdminRole.Admin);

        var token = _sut.GenerateAccessToken(adminUser);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.That(jwt.Issuer, Is.EqualTo(_settings.Issuer));
        Assert.That(jwt.Audiences, Does.Contain(_settings.Audience));
    }
}
