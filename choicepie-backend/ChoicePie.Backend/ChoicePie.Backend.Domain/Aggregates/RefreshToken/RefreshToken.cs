using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.RefreshToken;

public sealed class RefreshToken : AggregateRoot<Guid>
{
    private const int TokenLifetimeDays = 30;

    public Guid OwnerId { get; private set; }
    public RefreshTokenOwnerType OwnerType { get; private set; } = null!;
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTime.UtcNow;

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
