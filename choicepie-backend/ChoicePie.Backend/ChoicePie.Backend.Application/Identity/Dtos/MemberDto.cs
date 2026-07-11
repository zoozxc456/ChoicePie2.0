using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.Member;

namespace ChoicePie.Backend.Application.Identity.Dtos;

public sealed record MemberDto(Guid Id, string Email, string Name, string? Avatar, bool IsVerified, DateTime CreatedAt)
{
    public static MemberDto FromDomain(Member member, AuthAccount authAccount) =>
        new(member.Id, authAccount.Email.Value, member.Name, member.Avatar, authAccount.IsVerified, member.CreatedAt);
}
