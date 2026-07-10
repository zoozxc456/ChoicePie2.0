using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Application.Identity.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member.Specifications;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class RegisterMemberCommandHandler(
    IMemberRepository memberRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterMemberCommand, MemberDto>
{
    public async Task<MemberDto> Handle(RegisterMemberCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        var alreadyRegistered = await memberRepository.ExistsAsync(new MemberByEmailSpecification(email), cancellationToken);
        if (alreadyRegistered)
        {
            throw new EmailAlreadyRegisteredException(email.Value);
        }

        var passwordHash = passwordHasher.Hash(request.Password);
        var member = Member.Register(email, request.Name, passwordHash);

        await memberRepository.AddAsync(member, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MemberDto.FromDomain(member);
    }
}
