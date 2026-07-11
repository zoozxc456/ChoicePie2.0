using ChoicePie.Backend.Application.Identity.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class RegisterMemberCommandHandler(
    IMemberRepository memberRepository,
    IAuthAccountRepository authAccountRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterMemberCommand, MemberDto>
{
    public async Task<MemberDto> Handle(RegisterMemberCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        var alreadyRegistered =
            await authAccountRepository.ExistsAsync(new AuthAccountByEmailSpecification(email), cancellationToken);
        if (alreadyRegistered)
        {
            throw new EmailAlreadyRegisteredException(email.Value);
        }

        var member = Member.Create(request.Name);
        var (passwordHash, salt) = passwordHasher.Hash(request.Password);
        var authAccount = AuthAccount.Register(email, passwordHash, salt, member.Id);

        await memberRepository.AddAsync(member, cancellationToken);
        await authAccountRepository.AddAsync(authAccount, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MemberDto.FromDomain(member, authAccount);
    }
}