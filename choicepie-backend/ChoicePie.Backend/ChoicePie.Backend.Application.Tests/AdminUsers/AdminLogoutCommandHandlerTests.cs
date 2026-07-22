using ChoicePie.Backend.Application.AdminUsers.Commands;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;
using RefreshTokenAggregate = ChoicePie.Backend.Domain.Aggregates.RefreshToken.RefreshToken;

namespace ChoicePie.Backend.Application.Tests.AdminUsers;

[TestFixture]
public class AdminLogoutCommandHandlerTests
{
    private IRefreshTokenRepository _refreshTokenRepository = null!;
    private IRefreshTokenGenerator _refreshTokenGenerator = null!;
    private IUnitOfWork _unitOfWork = null!;
    private AdminLogoutCommandHandler _sut = null!;
    private RefreshTokenAggregate _existingRefreshToken = null!;

    [SetUp]
    public void SetUp()
    {
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new AdminLogoutCommandHandler(_refreshTokenRepository, _refreshTokenGenerator, _unitOfWork);

        _existingRefreshToken =
            RefreshTokenAggregate.Issue(Guid.NewGuid(), RefreshTokenOwnerType.Admin, "old-hash", DateTime.UtcNow);
        _refreshTokenGenerator.Hash("valid-raw-token").Returns("old-hash");
        _refreshTokenRepository
            .FirstOrDefaultAsync(Arg.Any<RefreshTokenByTokenHashSpecification>(), Arg.Any<CancellationToken>())
            .Returns(_existingRefreshToken);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    [Test]
    public async Task Handle_GivenActiveRefreshToken_WhenCalled_ThenRevokesToken()
    {
        await _sut.Handle(new AdminLogoutCommand { RefreshToken = "valid-raw-token" }, CancellationToken.None);

        Assert.That(_existingRefreshToken.RevokedAt, Is.Not.Null);
        await _refreshTokenRepository.Received(1).UpdateAsync(_existingRefreshToken, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenUnknownToken_WhenCalled_ThenDoesNothing()
    {
        _refreshTokenRepository
            .FirstOrDefaultAsync(Arg.Any<RefreshTokenByTokenHashSpecification>(), Arg.Any<CancellationToken>())
            .Returns((RefreshTokenAggregate?)null);

        await _sut.Handle(new AdminLogoutCommand { RefreshToken = "unknown-token" }, CancellationToken.None);

        await _refreshTokenRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<RefreshTokenAggregate>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
