using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Specifications;
using RefreshTokenAggregate = ChoicePie.Backend.Domain.Aggregates.RefreshToken.RefreshToken;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.RefreshToken.Specifications;

[TestFixture]
public class RefreshTokenByTokenHashSpecificationTests
{
    [Test]
    public void ToExpression_GivenRefreshTokenWithMatchingHash_WhenCompiledAndInvoked_ThenReturnsTrue()
    {
        var refreshToken = RefreshTokenAggregate.Issue(Guid.NewGuid(), RefreshTokenOwnerType.Member, "matching-hash",
            DateTime.UtcNow);
        var specification = new RefreshTokenByTokenHashSpecification("matching-hash");

        var isMatch = specification.ToExpression().Compile()(refreshToken);

        Assert.That(isMatch, Is.True);
    }

    [Test]
    public void ToExpression_GivenRefreshTokenWithDifferentHash_WhenCompiledAndInvoked_ThenReturnsFalse()
    {
        var refreshToken = RefreshTokenAggregate.Issue(Guid.NewGuid(), RefreshTokenOwnerType.Member, "matching-hash",
            DateTime.UtcNow);
        var specification = new RefreshTokenByTokenHashSpecification("different-hash");

        var isMatch = specification.ToExpression().Compile()(refreshToken);

        Assert.That(isMatch, Is.False);
    }
}
