using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.PasswordResetToken.Specifications;

public sealed class PasswordResetTokenByTokenHashSpecification(string tokenHash)
    : Specification<PasswordResetToken>(t => t.TokenHash == tokenHash);
