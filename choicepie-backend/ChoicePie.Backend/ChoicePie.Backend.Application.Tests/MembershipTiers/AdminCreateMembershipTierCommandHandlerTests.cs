using ChoicePie.Backend.Application.MembershipTiers.Commands;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.MembershipTiers;

[TestFixture]
public class AdminCreateMembershipTierCommandHandlerTests
{
    private IMembershipTierRepository _membershipTierRepository = null!;
    private IUnitOfWork _unitOfWork = null!;
    private AdminCreateMembershipTierCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _membershipTierRepository = Substitute.For<IMembershipTierRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new AdminCreateMembershipTierCommandHandler(_membershipTierRepository, _unitOfWork);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenValidInput_WhenCalled_ThenCreatesAndPersistsTier()
    {
        var command = new AdminCreateMembershipTierCommand("Pro", 10, 50_000);

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo("Pro"));
            Assert.That(result.DailyGenerationLimit, Is.EqualTo(10));
            Assert.That(result.DailyTokenBudget, Is.EqualTo(50_000));
        });
        await _membershipTierRepository.Received(1).AddAsync(Arg.Any<MembershipTier>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenInvalidName_WhenCalled_ThenThrowsInvalidMembershipTierException()
    {
        var command = new AdminCreateMembershipTierCommand("", 10, 50_000);

        Assert.ThrowsAsync<InvalidMembershipTierException>(() => _sut.Handle(command, CancellationToken.None));
    }
}
