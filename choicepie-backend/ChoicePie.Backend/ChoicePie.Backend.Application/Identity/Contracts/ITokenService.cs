using ChoicePie.Backend.Domain.Aggregates.Member;

namespace ChoicePie.Backend.Application.Identity.Contracts;

public interface ITokenService
{
    string GenerateAccessToken(Member member);
}
