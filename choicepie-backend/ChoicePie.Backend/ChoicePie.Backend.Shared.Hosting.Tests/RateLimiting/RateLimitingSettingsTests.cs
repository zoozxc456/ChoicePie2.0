using ChoicePie.Backend.Shared.Hosting.RateLimiting;

namespace ChoicePie.Backend.Shared.Hosting.Tests.RateLimiting;

[TestFixture]
public class RateLimitingSettingsTests
{
    [Test]
    public void Defaults_GivenNewSettings_WhenRead_ThenMatchAgreedLimits()
    {
        var sut = new RateLimitingSettings();

        Assert.That((sut.Auth.PermitLimit, sut.Auth.WindowSeconds), Is.EqualTo((10, 60)));
        Assert.That((sut.Email.PermitLimit, sut.Email.WindowSeconds), Is.EqualTo((5, 900)));
        Assert.That((sut.Ai.PermitLimit, sut.Ai.WindowSeconds), Is.EqualTo((5, 60)));
        Assert.That((sut.Write.PermitLimit, sut.Write.WindowSeconds), Is.EqualTo((30, 60)));
        Assert.That((sut.Global.PermitLimit, sut.Global.WindowSeconds), Is.EqualTo((300, 60)));
    }
}
