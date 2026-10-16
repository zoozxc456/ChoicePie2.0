using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Specifications;
using RefreshTokenAggregate = ChoicePie.Backend.Domain.Aggregates.RefreshToken.RefreshToken;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.RefreshToken.Specifications;

[TestFixture]
public class ActiveRefreshTokensByOwnerSpecificationTests
{
    private static readonly DateTime Now = DateTime.UtcNow;

    private static bool Matches(RefreshTokenAggregate token, Guid ownerId, RefreshTokenOwnerType? ownerType = null) =>
        new ActiveRefreshTokensByOwnerSpecification(ownerId, ownerType ?? RefreshTokenOwnerType.Member, Now)
            .ToExpression().Compile()(token);

    [Test]
    public void ToExpression_GivenLiveTokenOfOwner_WhenCompiledAndInvoked_ThenReturnsTrue()
    {
        var ownerId = Guid.NewGuid();
        var token = RefreshTokenAggregate.Issue(ownerId, RefreshTokenOwnerType.Member, "hash", Now);

        Assert.That(Matches(token, ownerId), Is.True);
    }

    [Test]
    public void ToExpression_GivenRevokedToken_WhenCompiledAndInvoked_ThenReturnsFalse()
    {
        var ownerId = Guid.NewGuid();
        var token = RefreshTokenAggregate.Issue(ownerId, RefreshTokenOwnerType.Member, "hash", Now);
        token.Revoke(Now);

        Assert.That(Matches(token, ownerId), Is.False);
    }

    [Test]
    public void ToExpression_GivenExpiredToken_WhenCompiledAndInvoked_ThenReturnsFalse()
    {
        var ownerId = Guid.NewGuid();
        var token = RefreshTokenAggregate.Issue(ownerId, RefreshTokenOwnerType.Member, "hash", Now.AddDays(-60));

        Assert.That(Matches(token, ownerId), Is.False);
    }

    [Test]
    public void ToExpression_GivenTokenOfOtherOwnerOrType_WhenCompiledAndInvoked_ThenReturnsFalse()
    {
        var ownerId = Guid.NewGuid();
        var token = RefreshTokenAggregate.Issue(ownerId, RefreshTokenOwnerType.Admin, "hash", Now);

        Assert.Multiple(() =>
        {
            Assert.That(Matches(token, Guid.NewGuid(), RefreshTokenOwnerType.Admin), Is.False);
            Assert.That(Matches(token, ownerId, RefreshTokenOwnerType.Member), Is.False);
        });
    }
}
