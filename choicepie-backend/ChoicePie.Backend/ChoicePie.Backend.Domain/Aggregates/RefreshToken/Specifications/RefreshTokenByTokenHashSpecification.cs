using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.RefreshToken.Specifications;

public sealed class RefreshTokenByTokenHashSpecification(string tokenHash)
    : Specification<RefreshToken>(t => t.TokenHash == tokenHash);
