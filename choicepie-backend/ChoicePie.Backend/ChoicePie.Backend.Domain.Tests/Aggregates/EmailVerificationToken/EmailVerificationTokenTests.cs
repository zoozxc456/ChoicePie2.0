using ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.Events;
using ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.Exceptions;
using EmailVerificationTokenAggregate = ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.EmailVerificationToken;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.EmailVerificationToken;

[TestFixture]
public class EmailVerificationTokenTests
{
    private static readonly Guid AuthAccountId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static EmailVerificationTokenAggregate Issue() =>
        EmailVerificationTokenAggregate.Issue(AuthAccountId, "user@example.com", "raw-token", "token-hash", Now);

    [Test]
    public void Issue_GivenValidInput_WhenCalled_ThenCreatesTokenWithExpectedFields()
    {
        var token = Issue();

        Assert.Multiple(() =>
        {
            Assert.That(token.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(token.AuthAccountId, Is.EqualTo(AuthAccountId));
            Assert.That(token.TokenHash, Is.EqualTo("token-hash"));
            Assert.That(token.ExpiresAt, Is.EqualTo(Now.AddHours(24)));
            Assert.That(token.IsActive, Is.True);
        });
    }

    [Test]
    public void Issue_GivenValidInput_WhenCalled_ThenRaisesEmailVerificationRequestedDomainEvent()
    {
        var token = Issue();

        var domainEvent = token.DomainEvents.OfType<EmailVerificationRequestedDomainEvent>().Single();
        Assert.Multiple(() =>
        {
            Assert.That(domainEvent.AuthAccountId, Is.EqualTo(AuthAccountId));
            Assert.That(domainEvent.Email, Is.EqualTo("user@example.com"));
            Assert.That(domainEvent.RawToken, Is.EqualTo("raw-token"));
        });
    }

    [Test]
    public void EnsureUsable_GivenActiveUnexpiredToken_WhenCalled_ThenDoesNotThrow()
    {
        var token = Issue();

        Assert.DoesNotThrow(() => token.EnsureUsable(Now.AddHours(1)));
    }

    [Test]
    public void EnsureUsable_GivenExpiredToken_WhenCalled_ThenThrowsInvalidEmailVerificationTokenException()
    {
        var token = Issue();

        Assert.Throws<InvalidEmailVerificationTokenException>(() => token.EnsureUsable(Now.AddHours(25)));
    }

    [Test]
    public void EnsureUsable_GivenUsedToken_WhenCalled_ThenThrowsInvalidEmailVerificationTokenException()
    {
        var token = Issue();
        token.MarkUsed(Now);

        Assert.Throws<InvalidEmailVerificationTokenException>(() => token.EnsureUsable(Now));
    }

    [Test]
    public void MarkUsed_GivenActiveToken_WhenCalled_ThenSetsIsActiveFalse()
    {
        var token = Issue();

        token.MarkUsed(Now);

        Assert.That(token.IsActive, Is.False);
    }
}
