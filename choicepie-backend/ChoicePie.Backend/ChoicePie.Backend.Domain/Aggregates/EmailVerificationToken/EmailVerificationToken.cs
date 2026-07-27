using ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.Events;
using ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken;

public sealed class EmailVerificationToken : AggregateRoot<Guid>
{
    private const int TokenLifetimeHours = 24;

    public Guid AuthAccountId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }

    public bool IsActive => UsedAt is null;

    private EmailVerificationToken()
    {
    }

    public static EmailVerificationToken Issue(
        Guid authAccountId, string email, string rawToken, string tokenHash, DateTime issuedAtUtc)
    {
        var token = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            AuthAccountId = authAccountId,
            TokenHash = tokenHash,
            ExpiresAt = issuedAtUtc.AddHours(TokenLifetimeHours)
        };

        token.SetCreated(authAccountId);
        token.AddDomainEvent(new EmailVerificationRequestedDomainEvent(authAccountId, email, rawToken));

        return token;
    }

    public void EnsureUsable(DateTime nowUtc)
    {
        if (UsedAt is not null || ExpiresAt <= nowUtc)
        {
            throw new InvalidEmailVerificationTokenException();
        }
    }

    public void MarkUsed(DateTime utcNow)
    {
        UsedAt = utcNow;
        Touch();
    }
}
