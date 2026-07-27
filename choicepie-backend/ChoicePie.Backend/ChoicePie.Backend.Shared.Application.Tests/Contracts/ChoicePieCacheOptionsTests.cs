using ChoicePie.Backend.Shared.Application.Abstractions.Caching;

namespace ChoicePie.Backend.Shared.Application.Tests.Contracts;

[TestFixture]
public class ChoicePieCacheOptionsTests
{
    [Test]
    public void Default_WhenRead_ThenReturnsFiveMinuteExpirationAndOneMinuteLocalExpiration()
    {
        var options = ChoicePieCacheOptions.Default;

        Assert.That(options.Expiration, Is.EqualTo(TimeSpan.FromMinutes(5)));
        Assert.That(options.LocalCacheExpiration, Is.EqualTo(TimeSpan.FromMinutes(1)));
    }

    [Test]
    public void Default_WhenCalledTwice_ThenReturnsDistinctInstancesNotASharedSingleton()
    {
        var first = ChoicePieCacheOptions.Default;
        var second = ChoicePieCacheOptions.Default;
        first.Expiration = TimeSpan.FromHours(1);

        Assert.That(second.Expiration, Is.EqualTo(TimeSpan.FromMinutes(5)), "Default 應該每次回傳新物件，避免呼叫端互相汙染");
    }
}
