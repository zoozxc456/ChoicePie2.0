using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Application.Identity.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.Exceptions;
using MediatR;
using RefreshTokenAggregate = ChoicePie.Backend.Domain.Aggregates.RefreshToken.RefreshToken;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IAuthAccountRepository authAccountRepository,
    IMemberRepository memberRepository,
    ITokenService tokenService,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<RefreshTokenCommand, LoginResultDto>
{
    public async Task<LoginResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = refreshTokenGenerator.Hash(request.RefreshToken);
        var existingToken =
            await refreshTokenRepository.FirstOrDefaultAsync(new RefreshTokenByTokenHashSpecification(tokenHash),
                cancellationToken)
            ?? throw new InvalidRefreshTokenException();

        if (!existingToken.IsActive || existingToken.OwnerType != RefreshTokenOwnerType.Member)
        {
            throw new InvalidRefreshTokenException();
        }

        var authAccount = await authAccountRepository.FirstOrDefaultAsync(
                               new AuthAccountByMemberIdSpecification(existingToken.OwnerId), cancellationToken)
                           ?? throw new AuthAccountNotFoundException(existingToken.OwnerId);
        var member = await memberRepository.GetByIdAsync(existingToken.OwnerId, cancellationToken)
                     ?? throw new MemberNotFoundException(existingToken.OwnerId);

        var now = timeProvider.GetUtcNow().UtcDateTime;

        // 停權中的會員不能換發新 token；以 401 拒絕（而非 403），讓 BFF 清掉 session、前端回到登入頁。
        // 已刪除的會員由軟刪除 query filter 擋下（查不到 → NotFound），且關閉帳號時 refresh token 已全數撤銷。
        if (member.IsCurrentlySuspended(now))
        {
            throw new InvalidRefreshTokenException();
        }

        var accessToken = tokenService.GenerateAccessToken(member);
        var (rawRefreshToken, refreshTokenHash) = refreshTokenGenerator.Generate();
        var newRefreshToken =
            RefreshTokenAggregate.Issue(member.Id, RefreshTokenOwnerType.Member, refreshTokenHash, now);

        existingToken.Revoke(now, newRefreshToken.Id);

        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            // 同一顆 token 被另一個請求搶先改寫。重新讀取後交給聚合判斷：若是剛被輪替、仍在寬限期內
            // （同一次 SSR 的並行 refresh），照樣換發——跟「晚一點才讀到已輪替 token」的請求結果一致；
            // 若是被登出或寬限期已過，就是無效 token。
            await refreshTokenRepository.ReloadAsync(existingToken, cancellationToken);
            if (!existingToken.IsActive)
            {
                throw new InvalidRefreshTokenException();
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new LoginResultDto(MemberDto.FromDomain(member, authAccount), accessToken, rawRefreshToken);
    }
}
