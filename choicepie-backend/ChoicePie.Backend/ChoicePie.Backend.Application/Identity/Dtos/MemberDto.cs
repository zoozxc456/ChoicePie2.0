using ChoicePie.Backend.Domain.Aggregates.Member;

namespace ChoicePie.Backend.Application.Identity.Dtos;

public sealed record MemberDto(Guid Id, string Email, string Name, string? Avatar, bool IsVerified, DateTime CreatedAt)
{
    public static MemberDto FromDomain(Member member) =>
        new(member.Id, member.Email.Value, member.Name, member.Avatar, member.IsVerified, member.CreatedAt);
}
