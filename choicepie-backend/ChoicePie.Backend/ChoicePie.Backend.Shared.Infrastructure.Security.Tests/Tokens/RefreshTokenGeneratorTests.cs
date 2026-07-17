using ChoicePie.Backend.Shared.Infrastructure.Security.Tokens;

namespace ChoicePie.Backend.Shared.Infrastructure.Security.Tests.Tokens;

[TestFixture]
public class RefreshTokenGeneratorTests
{
    private RefreshTokenGenerator _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new RefreshTokenGenerator();
    }

    [Test]
    public void Generate_WhenCalled_ThenReturnsNonEmptyRawTokenAndHash()
    {
        var (rawToken, tokenHash) = _sut.Generate();

        Assert.That(rawToken, Is.Not.Null.And.Not.Empty);
        Assert.That(tokenHash, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void Generate_WhenCalledTwice_ThenProducesDifferentRawTokens()
    {
        var (firstRaw, _) = _sut.Generate();
        var (secondRaw, _) = _sut.Generate();

        Assert.That(firstRaw, Is.Not.EqualTo(secondRaw));
    }

    [Test]
    public void Generate_WhenCalled_ThenReturnedHashMatchesHashingTheRawTokenSeparately()
    {
        var (rawToken, tokenHash) = _sut.Generate();

        Assert.That(_sut.Hash(rawToken), Is.EqualTo(tokenHash));
    }

    [Test]
    public void Hash_GivenSameRawToken_WhenCalledTwice_ThenReturnsSameHash()
    {
        var (rawToken, _) = _sut.Generate();

        var first = _sut.Hash(rawToken);
        var second = _sut.Hash(rawToken);

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public void Hash_GivenDifferentRawTokens_WhenCalled_ThenReturnsDifferentHashes()
    {
        var hashA = _sut.Hash("raw-token-a");
        var hashB = _sut.Hash("raw-token-b");

        Assert.That(hashA, Is.Not.EqualTo(hashB));
    }

    [Test]
    public void Hash_GivenRawToken_WhenCalled_ThenNeverEqualsTheRawTokenItself()
    {
        // 確保存進資料庫的是雜湊值而不是明文 refresh token（RefreshTokenRepository 只存 TokenHash）。
        const string rawToken = "some-raw-refresh-token-value";

        var hash = _sut.Hash(rawToken);

        Assert.That(hash, Is.Not.EqualTo(rawToken));
    }
}
