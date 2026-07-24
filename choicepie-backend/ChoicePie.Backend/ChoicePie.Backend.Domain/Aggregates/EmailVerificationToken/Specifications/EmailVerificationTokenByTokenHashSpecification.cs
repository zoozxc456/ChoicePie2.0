using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.Specifications;

public sealed class EmailVerificationTokenByTokenHashSpecification(string tokenHash)
    : Specification<EmailVerificationToken>(t => t.TokenHash == tokenHash);
