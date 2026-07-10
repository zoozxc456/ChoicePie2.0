using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Entities;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Events;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount;

public sealed class AuthAccount : AggregateRoot<Guid>
{
    private readonly List<LoginMethod> _loginMethods = [];

    public Email Email { get; private set; } = null!;
    public Guid MemberId { get; private set; }
    public bool IsVerified { get; private set; }
    public IReadOnlyList<LoginMethod> LoginMethods => _loginMethods.AsReadOnly();

    public string? OriginalPasswordHash =>
        _loginMethods.SingleOrDefault(m => m.Provider == LoginProvider.Original)?.PasswordHash;

    private AuthAccount()
    {
    }

    public static AuthAccount Register(Email email, string passwordHash, Guid memberId)
    {
        var authAccount = new AuthAccount
        {
            Id = Guid.NewGuid(),
            Email = email,
            MemberId = memberId,
            IsVerified = false
        };

        authAccount.SetCreated(authAccount.Id);
        authAccount._loginMethods.Add(LoginMethod.CreateOriginal(passwordHash));
        authAccount.AddDomainEvent(new AuthAccountRegisteredDomainEvent(authAccount.Id, memberId, email.Value));

        return authAccount;
    }

    public void AddLoginMethod(LoginProvider provider, string providerUserId)
    {
        if (_loginMethods.Any(m => m.Provider == provider))
        {
            throw new LoginMethodAlreadyLinkedException(provider);
        }

        _loginMethods.Add(LoginMethod.CreateExternal(provider, providerUserId));
    }
}
