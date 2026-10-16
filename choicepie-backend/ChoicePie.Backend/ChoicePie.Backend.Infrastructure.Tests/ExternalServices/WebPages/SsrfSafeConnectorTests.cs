using System.Net;
using ChoicePie.Backend.Infrastructure.ExternalServices.WebPages;

namespace ChoicePie.Backend.Infrastructure.Tests.ExternalServices.WebPages;

[TestFixture]
public class SsrfSafeConnectorTests
{
    private static SsrfSafeConnector CreateSut(string[] resolvedAddresses, List<IPAddress> connectedTo)
        => new(
            resolve: (_, _) => Task.FromResult(resolvedAddresses.Select(IPAddress.Parse).ToArray()),
            connect: (address, _, _) =>
            {
                connectedTo.Add(address);
                return Task.FromResult<Stream>(new MemoryStream());
            });

    [Test]
    public async Task ConnectAsync_GivenPublicAddress_WhenCalled_ThenConnectsToTheValidatedAddress()
    {
        var connectedTo = new List<IPAddress>();
        var sut = CreateSut(["93.184.216.34"], connectedTo);

        await using var stream = await sut.ConnectAsync(new DnsEndPoint("example.com", 443), CancellationToken.None);

        Assert.That(connectedTo, Is.EqualTo(new[] { IPAddress.Parse("93.184.216.34") }));
    }

    [TestCase("127.0.0.1")]
    [TestCase("169.254.169.254")]
    [TestCase("10.0.0.1")]
    [TestCase("::1")]
    [TestCase("::ffff:192.168.1.1")]
    public void ConnectAsync_GivenHostResolvingToBlockedAddress_WhenCalled_ThenThrowsBlockedAndNeverConnects(string address)
    {
        var connectedTo = new List<IPAddress>();
        var sut = CreateSut([address], connectedTo);

        Assert.ThrowsAsync<BlockedAddressException>(
            async () => await sut.ConnectAsync(new DnsEndPoint("evil.example.com", 80), CancellationToken.None));
        Assert.That(connectedTo, Is.Empty);
    }

    [Test]
    public void ConnectAsync_GivenOneBlockedAddressAmongPublicOnes_WhenCalled_ThenRejectsAllAndNeverConnects()
    {
        var connectedTo = new List<IPAddress>();
        var sut = CreateSut(["93.184.216.34", "127.0.0.1"], connectedTo);

        Assert.ThrowsAsync<BlockedAddressException>(
            async () => await sut.ConnectAsync(new DnsEndPoint("rebind.example.com", 80), CancellationToken.None));
        Assert.That(connectedTo, Is.Empty);
    }

    [Test]
    public void ConnectAsync_GivenHostResolvingToNothing_WhenCalled_ThenThrowsIOException()
    {
        var sut = CreateSut([], []);

        Assert.ThrowsAsync<IOException>(
            async () => await sut.ConnectAsync(new DnsEndPoint("nx.example.com", 80), CancellationToken.None));
    }

    [Test]
    public void CreateHandler_WhenCalled_ThenDisablesRedirectsAndProxy()
    {
        using var handler = SsrfSafeConnector.CreateHandler();

        Assert.Multiple(() =>
        {
            Assert.That(handler.AllowAutoRedirect, Is.False);
            Assert.That(handler.UseProxy, Is.False);
            Assert.That(handler.ConnectCallback, Is.Not.Null);
        });
    }
}
