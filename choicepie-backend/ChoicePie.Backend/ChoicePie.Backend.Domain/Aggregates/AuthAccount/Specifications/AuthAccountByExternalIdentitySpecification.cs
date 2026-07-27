using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;

public sealed class AuthAccountByExternalIdentitySpecification(LoginProvider provider, string providerUserId)
    : Specification<AuthAccount>(a =>
        a.LoginMethods.Any(m => m.External != null && m.External.Provider == provider &&
                                 m.External.ProviderUserId == providerUserId));
