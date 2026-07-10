using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Application.Identity.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class LoginCommandHandler(
    IAuthAccountRepository authAccountRepository,
    IMemberRepository memberRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService)
    : IRequestHandler<LoginCommand, LoginResultDto>
{
    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var authAccount = await authAccountRepository.FirstOrDefaultAsync(new AuthAccountByEmailSpecification(email), cancellationToken)
                           ?? throw new InvalidCredentialsException();

        if (authAccount.OriginalPasswordHash is not { } passwordHash || !passwordHasher.Verify(request.Password, passwordHash))
        {
            throw new InvalidCredentialsException();
        }

        var member = await memberRepository.GetByIdAsync(authAccount.MemberId, cancellationToken)
                     ?? throw new MemberNotFoundException(authAccount.MemberId);

        var token = tokenService.GenerateToken(member);

        return new LoginResultDto(MemberDto.FromDomain(member, authAccount), token);
    }
}
