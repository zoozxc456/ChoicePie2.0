using ChoicePie.Backend.Domain.Aggregates.MembershipTier.Exceptions;
using ChoicePie.Backend.Infrastructure.QueryServices.MembershipTiers;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using NSubstitute;
using MembershipTierAggregate = ChoicePie.Backend.Domain.Aggregates.MembershipTier.MembershipTier;

namespace ChoicePie.Backend.Infrastructure.Tests.QueryServices.MembershipTiers;

[TestFixture]
public class MembershipTierQueryServiceTests
{
    private IReadRepository _readRepository = null!;
    private MembershipTierQueryService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _readRepository = Substitute.For<IReadRepository>();
        _sut = new MembershipTierQueryService(_readRepository);
    }

    [Test]
    public async Task ListAsync_GivenMultipleTiers_WhenCalled_ThenReturnsAllOrderedByGenerationLimit()
    {
        var pro = MembershipTierAggregate.Create("Pro", 10, 50_000);
        var free = MembershipTierAggregate.Create("Free", 3, 10_000, isDefault: true);
        _readRepository.Query<MembershipTierAggregate>().Returns(new List<MembershipTierAggregate> { pro, free }.AsQueryable());

        var result = await _sut.ListAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("Free"));
            Assert.That(result[1].Name, Is.EqualTo("Pro"));
        });
    }

    [Test]
    public async Task GetByIdAsync_GivenExistingTier_WhenCalled_ThenReturnsDto()
    {
        var tier = MembershipTierAggregate.Create("Free", 3, 10_000, isDefault: true);
        _readRepository.Query<MembershipTierAggregate>().Returns(new List<MembershipTierAggregate> { tier }.AsQueryable());

        var result = await _sut.GetByIdAsync(tier.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo("Free"));
            Assert.That(result.IsDefault, Is.True);
        });
    }

    [Test]
    public void GetByIdAsync_GivenUnknownTierId_WhenCalled_ThenThrowsMembershipTierNotFoundException()
    {
        _readRepository.Query<MembershipTierAggregate>().Returns(new List<MembershipTierAggregate>().AsQueryable());

        Assert.ThrowsAsync<MembershipTierNotFoundException>(() => _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None));
    }
}
