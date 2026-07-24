using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Application.Identity.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using MediatR;
using RefreshTokenAggregate = ChoicePie.Backend.Domain.Aggregates.RefreshToken.RefreshToken;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class GoogleLoginCommandHandler(
    IGoogleIdTokenVerifier googleIdTokenVerifier,
    IAuthAccountRepository authAccountRepository,
    IMemberRepository memberRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ITokenService tokenService,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<GoogleLoginCommand, LoginResultDto>
{
    public async Task<LoginResultDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var googleIdentity = await googleIdTokenVerifier.VerifyAsync(request.IdToken, cancellationToken);
        var email = Email.Create(googleIdentity.Email);

        var authAccount = await authAccountRepository.FirstOrDefaultAsync(
            new AuthAccountByExternalIdentitySpecification(LoginProvider.Google, googleIdentity.ProviderUserId),
            cancellationToken);

        Member member;

        if (authAccount is not null)
        {
            member = await memberRepository.GetByIdAsync(authAccount.MemberId, cancellationToken)
                     ?? throw new MemberNotFoundException(authAccount.MemberId);
        }
        else
        {
            authAccount = await authAccountRepository.FirstOrDefaultAsync(
                new AuthAccountByEmailSpecification(email), cancellationToken);

            if (authAccount is not null)
            {
                authAccount.AddLoginMethod(LoginProvider.Google, googleIdentity.ProviderUserId);
                member = await memberRepository.GetByIdAsync(authAccount.MemberId, cancellationToken)
                         ?? throw new MemberNotFoundException(authAccount.MemberId);
                await authAccountRepository.UpdateAsync(authAccount, cancellationToken);
            }
            else
            {
                member = Member.Create(ClampToPersonNameLength(googleIdentity.Name));
                authAccount = AuthAccount.RegisterWithExternalLogin(
                    email, LoginProvider.Google, googleIdentity.ProviderUserId, member.Id);

                await memberRepository.AddAsync(member, cancellationToken);
                await authAccountRepository.AddAsync(authAccount, cancellationToken);
            }
        }

        if (member.IsCurrentlySuspended(timeProvider.GetUtcNow().UtcDateTime))
        {
            throw new MemberSuspendedException(member.Id, member.SuspendedReason);
        }

        var accessToken = tokenService.GenerateAccessToken(member);
        var (rawRefreshToken, refreshTokenHash) = refreshTokenGenerator.Generate();
        var refreshToken = RefreshTokenAggregate.Issue(
            member.Id, RefreshTokenOwnerType.Member, refreshTokenHash, timeProvider.GetUtcNow().UtcDateTime);

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResultDto(MemberDto.FromDomain(member, authAccount), accessToken, rawRefreshToken);
    }

    private static string ClampToPersonNameLength(string name)
    {
        var trimmed = name.Trim();
        if (trimmed.Length < PersonName.MinLength)
        {
            return trimmed.PadRight(PersonName.MinLength, '_');
        }

        return trimmed.Length > PersonName.MaxLength ? trimmed[..PersonName.MaxLength] : trimmed;
    }
}
