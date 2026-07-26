using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
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
    public void IsActive_GivenJustRotatedToken_WhenReadWithinGracePeriod_ThenStillReturnsTrue()
    {
        // Rotation 寬限期：同一次 SSR render 中 middleware 與頁面請求可能並行用舊 token 觸發 refresh，
        // 剛被替換的 token 需要在短暫緩衝期內仍視為有效，避免其中一個請求因競速而被拒絕。
        var refreshToken = CreateRefreshToken();

        refreshToken.Revoke(DateTime.UtcNow, Guid.NewGuid());

        Assert.That(refreshToken.IsActive, Is.True);
    }

    [Test]
    public void IsActive_GivenRotatedTokenPastGracePeriod_WhenRead_ThenReturnsFalse()
    {
        var refreshToken = CreateRefreshToken();

        refreshToken.Revoke(DateTime.UtcNow.AddSeconds(-31), Guid.NewGuid());

        Assert.That(refreshToken.IsActive, Is.False);
    }

    [Test]
    public void IsActive_GivenLoggedOutToken_WhenReadImmediately_ThenReturnsFalse()
    {
        // 登出（Revoke 不帶 replacedByTokenId）語意上必須立即失效，不套用 rotation 寬限期，
        // 否則使用者登出後短時間內仍能用同一顆 refresh token 換回新 session。
        var refreshToken = CreateRefreshToken();

        refreshToken.Revoke(DateTime.UtcNow);

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
    public void Revoke_GivenAlreadyRevokedToken_WhenCalledAgain_ThenIsNoOp()
    {
        // 併發的 401 → refresh 可能對同一顆已被輪替的 token 各自呼叫 Revoke；拋例外會讓其中一個
        // 呼叫端收到未預期的例外，這裡改為 no-op，維持第一次 revoke 記下的 RevokedAt/ReplacedByTokenId。
        var refreshToken = CreateRefreshToken();
        var firstRevokedAt = DateTime.UtcNow;
        var firstReplacementId = Guid.NewGuid();
        refreshToken.Revoke(firstRevokedAt, firstReplacementId);

        Assert.DoesNotThrow(() => refreshToken.Revoke(DateTime.UtcNow, Guid.NewGuid()));

        Assert.Multiple(() =>
        {
            Assert.That(refreshToken.RevokedAt, Is.EqualTo(firstRevokedAt));
            Assert.That(refreshToken.ReplacedByTokenId, Is.EqualTo(firstReplacementId));
        });
    }
}
