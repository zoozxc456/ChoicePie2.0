using System.Net;
using ChoicePie.Backend.Infrastructure.ExternalServices.WebPages;

namespace ChoicePie.Backend.Infrastructure.Tests.ExternalServices.WebPages;

[TestFixture]
public class WebPageUrlGuardTests
{
    [TestCase("https://example.com/article")]
    [TestCase("http://example.com/a?b=c#d")]
    [TestCase("https://example.com:443/")]
    [TestCase("http://example.com:80/")]
    [TestCase("https://93.184.216.34/")]
    [TestCase("https://[2606:4700:4700::1111]/")]
    [TestCase("https://中文.example.com/文章")]
    public void CheckUrl_GivenPublicHttpUrl_WhenChecked_ThenAllowed(string url)
    {
        Assert.That(WebPageUrlGuard.CheckUrl(new Uri(url)), Is.EqualTo(WebPageUrlCheck.Allowed));
    }

    [TestCase("ftp://example.com/file")]
    [TestCase("file:///etc/passwd")]
    [TestCase("gopher://example.com/")]
    [TestCase("javascript:alert(1)")]
    public void CheckUrl_GivenNonHttpScheme_WhenChecked_ThenUnsupported(string url)
    {
        Assert.That(WebPageUrlGuard.CheckUrl(new Uri(url)), Is.EqualTo(WebPageUrlCheck.Unsupported));
    }

    [Test]
    public void CheckUrl_GivenNullOrRelativeUri_WhenChecked_ThenUnsupported()
    {
        Assert.Multiple(() =>
        {
            Assert.That(WebPageUrlGuard.CheckUrl(null), Is.EqualTo(WebPageUrlCheck.Unsupported));
            Assert.That(WebPageUrlGuard.CheckUrl(new Uri("/relative", UriKind.Relative)), Is.EqualTo(WebPageUrlCheck.Unsupported));
        });
    }

    [TestCase("https://user:pass@example.com/")]
    [TestCase("https://user@example.com/")]
    [TestCase("http://example.com:8080/")]
    [TestCase("https://example.com:8443/")]
    [TestCase("http://example.com:22/")]
    [TestCase("http://example.com:443/")]
    [TestCase("https://example.com:80/")]
    public void CheckUrl_GivenUserInfoOrNonDefaultPort_WhenChecked_ThenDenied(string url)
    {
        Assert.That(WebPageUrlGuard.CheckUrl(new Uri(url)), Is.EqualTo(WebPageUrlCheck.Denied));
    }

    [TestCase("http://localhost/")]
    [TestCase("http://LOCALHOST/")]
    [TestCase("http://localhost./")]
    [TestCase("http://foo.localhost/")]
    [TestCase("http://127.0.0.1/")]
    [TestCase("http://127.1/")]
    [TestCase("http://2130706433/")]
    [TestCase("http://0x7f.0.0.1/")]
    [TestCase("http://[::1]/")]
    [TestCase("http://[::ffff:127.0.0.1]/")]
    [TestCase("http://[::ffff:7f00:1]/")]
    [TestCase("http://169.254.169.254/latest/meta-data/")]
    [TestCase("http://[::ffff:169.254.169.254]/")]
    [TestCase("http://10.0.0.5/")]
    [TestCase("http://192.168.1.1/")]
    [TestCase("http://[fd00::1]/")]
    public void CheckUrl_GivenInternalHostLiteral_WhenChecked_ThenDenied(string url)
    {
        Assert.That(WebPageUrlGuard.CheckUrl(new Uri(url)), Is.EqualTo(WebPageUrlCheck.Denied));
    }

    [TestCase("1.1.1.1")]
    [TestCase("8.8.8.8")]
    [TestCase("93.184.216.34")]
    [TestCase("172.15.255.255")]
    [TestCase("172.32.0.0")]
    [TestCase("100.63.255.255")]
    [TestCase("100.128.0.0")]
    [TestCase("169.253.0.1")]
    [TestCase("192.167.0.1")]
    [TestCase("223.255.255.255")]
    [TestCase("2606:4700:4700::1111")]
    [TestCase("2001:4860:4860::8888")]
    [TestCase("::ffff:8.8.8.8")]
    [TestCase("2002:0808:0808::1")]
    public void IsAllowedAddress_GivenPublicAddress_WhenChecked_ThenTrue(string address)
    {
        Assert.That(WebPageUrlGuard.IsAllowedAddress(IPAddress.Parse(address)), Is.True);
    }

    [TestCase("0.0.0.0")]
    [TestCase("0.255.255.255")]
    [TestCase("10.0.0.0")]
    [TestCase("10.255.255.255")]
    [TestCase("127.0.0.1")]
    [TestCase("127.255.255.254")]
    [TestCase("100.64.0.0")]
    [TestCase("100.127.255.255")]
    [TestCase("169.254.0.1")]
    [TestCase("169.254.169.254")]
    [TestCase("172.16.0.0")]
    [TestCase("172.31.255.255")]
    [TestCase("192.168.0.0")]
    [TestCase("192.168.255.255")]
    [TestCase("192.0.0.1")]
    [TestCase("198.18.0.1")]
    [TestCase("198.19.255.255")]
    [TestCase("224.0.0.1")]
    [TestCase("239.255.255.255")]
    [TestCase("240.0.0.1")]
    [TestCase("255.255.255.255")]
    [TestCase("::")]
    [TestCase("::1")]
    [TestCase("fc00::1")]
    [TestCase("fdff:ffff::1")]
    [TestCase("fe80::1")]
    [TestCase("febf::1")]
    [TestCase("fec0::1")]
    [TestCase("ff02::1")]
    [TestCase("::ffff:127.0.0.1")]
    [TestCase("::ffff:10.1.2.3")]
    [TestCase("::ffff:172.16.0.1")]
    [TestCase("::ffff:192.168.0.1")]
    [TestCase("::ffff:169.254.169.254")]
    [TestCase("::ffff:100.64.0.1")]
    [TestCase("::ffff:0.0.0.0")]
    [TestCase("::ffff:255.255.255.255")]
    [TestCase("::127.0.0.1")]
    [TestCase("64:ff9b::7f00:1")]
    [TestCase("64:ff9b::a9fe:a9fe")]
    [TestCase("2002:7f00:0001::1")]
    [TestCase("2002:a9fe:a9fe::1")]
    public void IsAllowedAddress_GivenInternalOrSpecialAddress_WhenChecked_ThenFalse(string address)
    {
        Assert.That(WebPageUrlGuard.IsAllowedAddress(IPAddress.Parse(address)), Is.False);
    }
}
