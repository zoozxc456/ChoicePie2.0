using ChoicePie.Backend.Domain.Aggregates.PasswordResetToken.Events;
using ChoicePie.Backend.Domain.Aggregates.PasswordResetToken.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.PasswordResetToken;

public sealed class PasswordResetToken : AggregateRoot<Guid>
{
    private const int TokenLifetimeMinutes = 30;

    public Guid AuthAccountId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }

    public bool IsActive => UsedAt is null;

    private PasswordResetToken()
    {
    }

    public static PasswordResetToken Issue(
        Guid authAccountId, string email, string rawToken, string tokenHash, DateTime issuedAtUtc)
    {
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            AuthAccountId = authAccountId,
            TokenHash = tokenHash,
            ExpiresAt = issuedAtUtc.AddMinutes(TokenLifetimeMinutes)
        };

        token.SetCreated(authAccountId);
        token.AddDomainEvent(new PasswordResetRequestedDomainEvent(authAccountId, email, rawToken));

        return token;
    }

    public void EnsureUsable(DateTime nowUtc)
    {
        if (UsedAt is not null || ExpiresAt <= nowUtc)
        {
            throw new InvalidPasswordResetTokenException();
        }
    }

    public void MarkUsed(DateTime utcNow)
    {
        UsedAt = utcNow;
        Touch();
    }
}
