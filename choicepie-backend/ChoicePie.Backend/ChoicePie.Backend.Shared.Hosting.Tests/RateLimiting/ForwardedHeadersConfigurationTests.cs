using ChoicePie.Backend.Shared.Hosting.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace ChoicePie.Backend.Shared.Hosting.Tests.RateLimiting;

[TestFixture]
public class ForwardedHeadersConfigurationTests
{
    [Test]
    public void Apply_GivenNoConfig_WhenCalled_ThenOnlyLoopbackIsTrusted()
    {
        var options = new ForwardedHeadersOptions();

        ChoicePieForwardedHeadersExtensions.Apply(options, new ConfigurationBuilder().Build());

        Assert.That(options.KnownProxies.Select(p => p.ToString()), Is.EquivalentTo(new[] { "127.0.0.1", "::1" }));
        Assert.That(options.KnownIPNetworks, Is.Empty);
        Assert.That(options.ForwardedHeaders, Is.EqualTo(Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor));
    }

    [Test]
    public void Apply_GivenKnownProxiesAndNetworks_WhenCalled_ThenTheyReplaceDefaults()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ForwardedHeaders:KnownProxies:0"] = "10.1.2.3",
            ["ForwardedHeaders:KnownNetworks:0"] = "172.16.0.0/12"
        }).Build();
        var options = new ForwardedHeadersOptions();

        ChoicePieForwardedHeadersExtensions.Apply(options, config);

        Assert.That(options.KnownProxies.Select(p => p.ToString()), Is.EquivalentTo(new[] { "10.1.2.3" }));
        Assert.That(options.KnownIPNetworks.Count, Is.EqualTo(1));
    }
}
