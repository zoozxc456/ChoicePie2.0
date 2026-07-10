using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Application.Identity.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member.Specifications;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class LoginCommandHandler(
    IMemberRepository memberRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService)
    : IRequestHandler<LoginCommand, LoginResultDto>
{
    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var member = await memberRepository.FirstOrDefaultAsync(new MemberByEmailSpecification(email), cancellationToken);

        if (member is null || !passwordHasher.Verify(request.Password, member.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        var token = tokenService.GenerateToken(member);

        return new LoginResultDto(MemberDto.FromDomain(member), token);
    }
}
