using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Exceptions;
using RefreshTokenAggregate = ChoicePie.Backend.Domain.Aggregates.RefreshToken.RefreshToken;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.RefreshToken;

[TestFixture]
public class RefreshTokenTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly DateTime IssuedAtUtc = DateTime.UtcNow;

    private static RefreshTokenAggregate CreateRefreshToken() =>
        RefreshTokenAggregate.Issue(OwnerId, RefreshTokenOwnerType.Member, "token-hash", IssuedAtUtc);

    [Test]
    public void Issue_GivenValidInput_WhenCalled_ThenCreatesRefreshTokenWithExpectedFields()
    {
        var refreshToken = CreateRefreshToken();

        Assert.Multiple(() =>
        {
            Assert.That(refreshToken.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(refreshToken.OwnerId, Is.EqualTo(OwnerId));
            Assert.That(refreshToken.OwnerType, Is.EqualTo(RefreshTokenOwnerType.Member));
            Assert.That(refreshToken.TokenHash, Is.EqualTo("token-hash"));
            Assert.That(refreshToken.ExpiresAt, Is.EqualTo(IssuedAtUtc.AddDays(30)));
            Assert.That(refreshToken.RevokedAt, Is.Null);
            Assert.That(refreshToken.ReplacedByTokenId, Is.Null);
        });
    }

    [Test]
    public void IsActive_GivenFreshlyIssuedToken_WhenRead_ThenReturnsTrue()
    {
        var refreshToken = CreateRefreshToken();

        Assert.That(refreshToken.IsActive, Is.True);
    }

    [Test]
    public void IsActive_GivenExpiredToken_WhenRead_ThenReturnsFalse()
    {
        var refreshToken = RefreshTokenAggregate.Issue(OwnerId, RefreshTokenOwnerType.Member, "token-hash",
            DateTime.UtcNow.AddDays(-31));

        Assert.That(refreshToken.IsActive, Is.False);
    }

    [Test]
    public void IsActive_GivenJustRevokedToken_WhenReadWithinGracePeriod_ThenStillReturnsTrue()
    {
        // Rotation 寬限期：同一次 SSR render 中 middleware 與頁面請求可能並行用舊 token 觸發 refresh，
        // 剛被替換的 token 需要在短暫緩衝期內仍視為有效，避免其中一個請求因競速而被拒絕。
        var refreshToken = CreateRefreshToken();

        refreshToken.Revoke(DateTime.UtcNow);

        Assert.That(refreshToken.IsActive, Is.True);
    }

    [Test]
    public void IsActive_GivenRevokedTokenPastGracePeriod_WhenRead_ThenReturnsFalse()
    {
        var refreshToken = CreateRefreshToken();

        refreshToken.Revoke(DateTime.UtcNow.AddSeconds(-31));

        Assert.That(refreshToken.IsActive, Is.False);
    }

    [Test]
    public void Revoke_GivenActiveToken_WhenCalled_ThenSetsRevokedAtAndReplacedByTokenId()
    {
        var refreshToken = CreateRefreshToken();
        var replacementId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        refreshToken.Revoke(now, replacementId);

        Assert.Multiple(() =>
        {
            Assert.That(refreshToken.RevokedAt, Is.EqualTo(now));
            Assert.That(refreshToken.ReplacedByTokenId, Is.EqualTo(replacementId));
        });
    }

    [Test]
    public void Revoke_GivenAlreadyRevokedToken_WhenCalledAgain_ThenThrowsInvalidRefreshTokenException()
    {
        var refreshToken = CreateRefreshToken();
        refreshToken.Revoke(DateTime.UtcNow);

        Assert.Throws<InvalidRefreshTokenException>(() => refreshToken.Revoke(DateTime.UtcNow));
    }
}
