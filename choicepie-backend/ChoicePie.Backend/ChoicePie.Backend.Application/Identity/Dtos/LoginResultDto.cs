namespace ChoicePie.Backend.Application.Identity.Dtos;

public sealed record LoginResultDto(MemberDto Member, string Token);
