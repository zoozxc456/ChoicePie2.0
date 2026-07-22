using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.RefreshToken.Enums;

[TestFixture]
public class RefreshTokenOwnerTypeTests
{
    [Test]
    public void FromName_GivenKnownNameDifferentCase_WhenCalled_ThenReturnsMatchingInstance()
    {
        var ownerType = RefreshTokenOwnerType.FromName("ADMIN");

        Assert.That(ownerType, Is.EqualTo(RefreshTokenOwnerType.Admin));
    }

    [Test]
    public void FromName_GivenUnknownName_WhenCalled_ThenReturnsNull()
    {
        Assert.That(RefreshTokenOwnerType.FromName("guest"), Is.Null);
    }

    [Test]
    public void FromValue_GivenKnownId_WhenCalled_ThenReturnsMatchingInstance()
    {
        Assert.That(RefreshTokenOwnerType.FromValue(1), Is.EqualTo(RefreshTokenOwnerType.Member));
    }

    [Test]
    public void Enumerations_WhenRead_ThenContainsExactlyTwoOwnerTypes()
    {
        Assert.That(RefreshTokenOwnerType.Enumerations.Values, Is.EquivalentTo(new[]
        {
            RefreshTokenOwnerType.Member, RefreshTokenOwnerType.Admin
        }));
    }
}
