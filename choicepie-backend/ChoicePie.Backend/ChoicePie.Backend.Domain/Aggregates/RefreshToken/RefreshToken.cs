using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.RefreshToken;

public sealed class RefreshToken : AggregateRoot<Guid>
{
    private const int TokenLifetimeDays = 30;

    // Refresh token rotation 的寬限期：同一次 SSR render 中，middleware 與頁面資料請求可能各自
    // 用同一顆舊 refresh token 並行觸發 refresh，若舊 token 一 revoke 就立刻失效，其中一個請求會
    // 因為輪替競速而失敗、把使用者踢回登入頁。被替換後的舊 token 在這段緩衝時間內仍視為有效，
    // 讓並行請求都能成功，緩衝期過後才真正失效，維持 rotation 的安全性（token 外洩仍會被淘汰）。
    private static readonly TimeSpan RevocationGracePeriod = TimeSpan.FromSeconds(30);

    public Guid OwnerId { get; private set; }
    public RefreshTokenOwnerType OwnerType { get; private set; } = null!;
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    public bool IsActive => ExpiresAt > DateTime.UtcNow
                             && (RevokedAt is null || DateTime.UtcNow < RevokedAt.Value.Add(RevocationGracePeriod));

    private RefreshToken()
    {
    }

    public static RefreshToken Issue(Guid ownerId, RefreshTokenOwnerType ownerType, string tokenHash,
        DateTime issuedAtUtc)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            OwnerType = ownerType,
            TokenHash = tokenHash,
            ExpiresAt = issuedAtUtc.AddDays(TokenLifetimeDays)
        };

        refreshToken.SetCreated(ownerId);

        return refreshToken;
    }

    public void Revoke(DateTime utcNow, Guid? replacedByTokenId = null)
    {
        if (RevokedAt is not null)
        {
            throw new InvalidRefreshTokenException();
        }

        RevokedAt = utcNow;
        ReplacedByTokenId = replacedByTokenId;
    }
}
