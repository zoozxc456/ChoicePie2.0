using ChoicePie.Backend.Application.Identity.Commands;
using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;
using RefreshTokenAggregate = ChoicePie.Backend.Domain.Aggregates.RefreshToken.RefreshToken;
using ChoicePie.Backend.Shared.Kernel.Exceptions;
using NSubstitute.ExceptionExtensions;

namespace ChoicePie.Backend.Application.Tests.Identity;

[TestFixture]
public class RefreshTokenCommandHandlerTests
{
    private IRefreshTokenRepository _refreshTokenRepository = null!;
    private IAuthAccountRepository _authAccountRepository = null!;
    private IMemberRepository _memberRepository = null!;
    private ITokenService _tokenService = null!;
    private IRefreshTokenGenerator _refreshTokenGenerator = null!;
    private IUnitOfWork _unitOfWork = null!;
    private TimeProvider _timeProvider = null!;
    private RefreshTokenCommandHandler _sut = null!;
    private Member _member = null!;
    private AuthAccount _authAccount = null!;
    private RefreshTokenAggregate _existingRefreshToken = null!;

    [SetUp]
    public void SetUp()
    {
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _authAccountRepository = Substitute.For<IAuthAccountRepository>();
        _memberRepository = Substitute.For<IMemberRepository>();
        _tokenService = Substitute.For<ITokenService>();
        _refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);
        _sut = new RefreshTokenCommandHandler(_refreshTokenRepository, _authAccountRepository, _memberRepository,
            _tokenService, _refreshTokenGenerator, _unitOfWork, _timeProvider);

        _member = Member.Create("Host Name");
        _authAccount = AuthAccount.Register(
            Email.Create("host@example.com"), HashedPassword.Create("hashed-password", "salt"), _member.Id);
        _existingRefreshToken =
            RefreshTokenAggregate.Issue(_member.Id, RefreshTokenOwnerType.Member, "old-hash", DateTime.UtcNow);

        _refreshTokenGenerator.Hash("valid-raw-token").Returns("old-hash");
        _refreshTokenRepository
            .FirstOrDefaultAsync(Arg.Any<RefreshTokenByTokenHashSpecification>(), Arg.Any<CancellationToken>())
            .Returns(_existingRefreshToken);
        _authAccountRepository
            .FirstOrDefaultAsync(Arg.Any<AuthAccountByMemberIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(_authAccount);
        _memberRepository.GetByIdAsync(_member.Id, Arg.Any<CancellationToken>()).Returns(_member);
        _refreshTokenGenerator.Generate().Returns(("new-raw-token", "new-hash"));
        _tokenService.GenerateAccessToken(_member).Returns("new-access-token");
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    private static RefreshTokenCommand ValidCommand() => new() { RefreshToken = "valid-raw-token" };

    [Test]
    public async Task Handle_GivenActiveRefreshToken_WhenCalled_ThenReturnsNewAccessAndRefreshTokens()
    {
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.AccessToken, Is.EqualTo("new-access-token"));
            Assert.That(result.RefreshToken, Is.EqualTo("new-raw-token"));
            Assert.That(result.Member.Email, Is.EqualTo("host@example.com"));
        });
    }

    [Test]
    public async Task Handle_GivenActiveRefreshToken_WhenCalled_ThenRevokesOldTokenAndPersistsNewOne()
    {
        await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_existingRefreshToken.RevokedAt, Is.Not.Null);
            Assert.That(_existingRefreshToken.ReplacedByTokenId, Is.Not.Null);
        });
        await _refreshTokenRepository.Received(1).AddAsync(
            Arg.Is<RefreshTokenAggregate>(t => t.TokenHash == "new-hash"), Arg.Any<CancellationToken>());
        await _refreshTokenRepository.Received(1).UpdateAsync(_existingRefreshToken, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenUnknownToken_WhenCalled_ThenThrowsInvalidRefreshTokenException()
    {
        _refreshTokenRepository
            .FirstOrDefaultAsync(Arg.Any<RefreshTokenByTokenHashSpecification>(), Arg.Any<CancellationToken>())
            .Returns((RefreshTokenAggregate?)null);

        Assert.ThrowsAsync<InvalidRefreshTokenException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenRevokedTokenPastGracePeriod_WhenCalled_ThenThrowsInvalidRefreshTokenException()
    {
        _existingRefreshToken.Revoke(DateTime.UtcNow.AddSeconds(-31));

        Assert.ThrowsAsync<InvalidRefreshTokenException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenAdminOwnedToken_WhenCalled_ThenThrowsInvalidRefreshTokenException()
    {
        var adminOwnedToken =
            RefreshTokenAggregate.Issue(Guid.NewGuid(), RefreshTokenOwnerType.Admin, "old-hash", DateTime.UtcNow);
        _refreshTokenRepository
            .FirstOrDefaultAsync(Arg.Any<RefreshTokenByTokenHashSpecification>(), Arg.Any<CancellationToken>())
            .Returns(adminOwnedToken);

        Assert.ThrowsAsync<InvalidRefreshTokenException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public async Task Handle_GivenPermanentlySuspendedMember_WhenCalled_ThenThrowsAndDoesNotIssueTokens()
    {
        _member.Suspend("spamming", null);

        Assert.ThrowsAsync<InvalidRefreshTokenException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.That(_existingRefreshToken.RevokedAt, Is.Null);
    }

    [Test]
    public void Handle_GivenSuspensionNotYetExpired_WhenCalled_ThenThrowsInvalidRefreshTokenException()
    {
        _member.Suspend("spamming", DateTime.UtcNow.AddHours(1));

        Assert.ThrowsAsync<InvalidRefreshTokenException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public async Task Handle_GivenSuspensionAlreadyExpired_WhenCalled_ThenIssuesNewTokens()
    {
        _member.Suspend("spamming", DateTime.UtcNow.AddHours(-1));

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.That(result.AccessToken, Is.EqualTo("new-access-token"));
    }

    // 同一次 SSR 的 middleware 與頁面請求會拿同一顆 token 同時 refresh。先存檔的那個輪替成功；
    // 後存檔的撞到 xmin 衝突，重新讀取後發現 token 是「剛被輪替、仍在寬限期內」，照樣換發，不能把使用者踢掉。
    [Test]
    public async Task Handle_GivenTokenRotatedConcurrentlyWithinGracePeriod_WhenSaving_ThenReloadsAndStillIssuesTokens()
    {
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(_ => throw new ConcurrencyConflictException(), _ => Task.FromResult(1));
        _refreshTokenRepository
            .When(r => r.ReloadAsync(_existingRefreshToken, Arg.Any<CancellationToken>()))
            .Do(_ => SimulateReloadedFromDatabase(revokedAt: DateTime.UtcNow, replacedByTokenId: Guid.NewGuid()));

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.That(result.RefreshToken, Is.EqualTo("new-raw-token"));
        await _unitOfWork.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // 搶先的是登出（撤銷但沒有替代 token）：登出必須立即生效，不能在寬限期內再換出新 session。
    [Test]
    public async Task Handle_GivenTokenLoggedOutConcurrently_WhenSaving_ThenThrowsInvalidRefreshTokenWithoutRetrying()
    {
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new ConcurrencyConflictException());
        _refreshTokenRepository
            .When(r => r.ReloadAsync(_existingRefreshToken, Arg.Any<CancellationToken>()))
            .Do(_ => SimulateReloadedFromDatabase(revokedAt: DateTime.UtcNow, replacedByTokenId: null));

        Assert.ThrowsAsync<InvalidRefreshTokenException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // EF 的 Reload 會用資料庫裡的值覆蓋記憶體中的聚合（包括 handler 自己剛做的 Revoke），
    // 單純再呼叫一次 Revoke 會因為已撤銷而直接 return，模擬不出「別人寫進去的狀態」。
    private void SimulateReloadedFromDatabase(DateTime revokedAt, Guid? replacedByTokenId)
    {
        typeof(RefreshTokenAggregate).GetProperty(nameof(RefreshTokenAggregate.RevokedAt))!
            .SetValue(_existingRefreshToken, revokedAt);
        typeof(RefreshTokenAggregate).GetProperty(nameof(RefreshTokenAggregate.ReplacedByTokenId))!
            .SetValue(_existingRefreshToken, replacedByTokenId);
    }
}
