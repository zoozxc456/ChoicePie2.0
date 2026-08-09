using ChoicePie.Backend.Application.MembershipTiers.Commands;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;
using MembershipTierAggregate = ChoicePie.Backend.Domain.Aggregates.MembershipTier.MembershipTier;

namespace ChoicePie.Backend.Application.Tests.MembershipTiers;

[TestFixture]
public class AdminUpdateMembershipTierCommandHandlerTests
{
    private IMembershipTierRepository _membershipTierRepository = null!;
    private IUnitOfWork _unitOfWork = null!;
    private AdminUpdateMembershipTierCommandHandler _sut = null!;
    private MembershipTierAggregate _tier = null!;

    [SetUp]
    public void SetUp()
    {
        _membershipTierRepository = Substitute.For<IMembershipTierRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new AdminUpdateMembershipTierCommandHandler(_membershipTierRepository, _unitOfWork);

        _tier = MembershipTierAggregate.Create("Free", 3, 10_000);
        _membershipTierRepository.GetByIdAsync(_tier.Id, Arg.Any<CancellationToken>()).Returns(_tier);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenValidInput_WhenCalled_ThenUpdatesAndPersistsTier()
    {
        var command = new AdminUpdateMembershipTierCommand(_tier.Id, "Pro", 10, 50_000);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo("Pro"));
            Assert.That(result.DailyGenerationLimit, Is.EqualTo(10));
            Assert.That(result.DailyTokenBudget, Is.EqualTo(50_000));
        });
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenTierNotFound_WhenCalled_ThenThrowsMembershipTierNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _membershipTierRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((MembershipTierAggregate?)null);

        Assert.ThrowsAsync<MembershipTierNotFoundException>(() =>
            _sut.Handle(new AdminUpdateMembershipTierCommand(missingId, "Pro", 10, 50_000), CancellationToken.None));
    }
}
