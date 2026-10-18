using ChoicePie.Backend.Application.Identity.Dtos;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.Exceptions;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class RegisterMemberCommandHandler(
    IMemberRepository memberRepository,
    IAuthAccountRepository authAccountRepository,
    IEmailVerificationTokenRepository emailVerificationTokenRepository,
    IMembershipTierRepository membershipTierRepository,
    IPasswordHasher passwordHasher,
    IRefreshTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
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
        await AssignDefaultTierAsync(member, cancellationToken);
        var hashedPassword = passwordHasher.Hash(request.Password);
        var authAccount = AuthAccount.Register(email, hashedPassword, member.Id);

        var (rawToken, tokenHash) = tokenGenerator.Generate();
        var verificationToken = EmailVerificationToken.Issue(
            authAccount.Id, email.Value, rawToken, tokenHash, timeProvider.GetUtcNow().UtcDateTime);

        await memberRepository.AddAsync(member, cancellationToken);
        await authAccountRepository.AddAsync(authAccount, cancellationToken);
        await emailVerificationTokenRepository.AddAsync(verificationToken, cancellationToken);
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            // 同一個 Email 同時註冊：兩邊都通過了上面的檢查，由唯一索引擋下較慢的那個。
            throw new EmailAlreadyRegisteredException(email.Value);
        }

        return MemberDto.FromDomain(member, authAccount);
    }

    // 沒有預設方案時維持未指派，AI 出題額度視同為 0。
    private async Task AssignDefaultTierAsync(Member member, CancellationToken ct)
    {
        var defaultTier = await membershipTierRepository.FirstOrDefaultAsync(new DefaultMembershipTierSpecification(), ct);
        if (defaultTier is not null)
        {
            member.AssignTier(defaultTier.Id);
        }
    }
}
