using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Application.Identity.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
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

public sealed class LoginCommandHandler(
    IAuthAccountRepository authAccountRepository,
    IMemberRepository memberRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LoginCommand, LoginResultDto>
{
    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var authAccount =
            await authAccountRepository.FirstOrDefaultAsync(new AuthAccountByEmailSpecification(email),
                cancellationToken)
            ?? throw new InvalidCredentialsException();

        if (authAccount.OriginalPassword is not { } hashedPassword ||
            !passwordHasher.Verify(request.Password, hashedPassword))
        {
            throw new InvalidCredentialsException();
        }

        var member = await memberRepository.GetByIdAsync(authAccount.MemberId, cancellationToken)
                     ?? throw new MemberNotFoundException(authAccount.MemberId);

        var accessToken = tokenService.GenerateAccessToken(member);
        var (rawRefreshToken, refreshTokenHash) = refreshTokenGenerator.Generate();
        var refreshToken =
            RefreshTokenAggregate.Issue(member.Id, RefreshTokenOwnerType.Member, refreshTokenHash, DateTime.UtcNow);

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResultDto(MemberDto.FromDomain(member, authAccount), accessToken, rawRefreshToken);
    }
}
