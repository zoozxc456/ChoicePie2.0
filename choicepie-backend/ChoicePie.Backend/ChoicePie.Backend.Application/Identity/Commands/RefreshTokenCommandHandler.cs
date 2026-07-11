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
using MediatR;
using RefreshTokenAggregate = ChoicePie.Backend.Domain.Aggregates.RefreshToken.RefreshToken;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IAuthAccountRepository authAccountRepository,
    IMemberRepository memberRepository,
    ITokenService tokenService,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork)
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

        var accessToken = tokenService.GenerateAccessToken(member);
        var (rawRefreshToken, refreshTokenHash) = refreshTokenGenerator.Generate();
        var newRefreshToken =
            RefreshTokenAggregate.Issue(member.Id, RefreshTokenOwnerType.Member, refreshTokenHash, DateTime.UtcNow);

        existingToken.Revoke(DateTime.UtcNow, newRefreshToken.Id);

        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResultDto(MemberDto.FromDomain(member, authAccount), accessToken, rawRefreshToken);
    }
}
