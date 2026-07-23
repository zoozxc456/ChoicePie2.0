using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Entities;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Enums;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Events;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;

namespace ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;

public sealed class AdminAuthAccount : AggregateRoot<Guid>
{
    private readonly List<AdminLoginMethod> _loginMethods = [];

    public Email Email { get; private set; } = null!;
    public Guid AdminUserId { get; private set; }
    public bool IsVerified { get; private set; }
    public IReadOnlyList<AdminLoginMethod> LoginMethods => _loginMethods.AsReadOnly();

    public HashedPassword? OriginalPassword =>
        _loginMethods.SingleOrDefault(m => m.Provider == AdminLoginProvider.Original)?.Password;

    private AdminAuthAccount()
    {
    }

    public static AdminAuthAccount Create(Email email, HashedPassword password, Guid adminUserId)
    {
        var adminAuthAccount = new AdminAuthAccount
        {
            Id = Guid.NewGuid(),
            Email = email,
            AdminUserId = adminUserId,
            IsVerified = false
        };

        adminAuthAccount.SetCreated(adminAuthAccount.Id);
        adminAuthAccount._loginMethods.Add(AdminLoginMethod.CreateOriginal(password));
        adminAuthAccount.AddDomainEvent(
            new AdminAuthAccountCreatedDomainEvent(adminAuthAccount.Id, adminUserId, email.Value));

        return adminAuthAccount;
    }

    public void AddLoginMethod(AdminLoginProvider provider, string providerUserId)
    {
        if (_loginMethods.Any(m => m.Provider == provider))
        {
            throw new AdminLoginMethodAlreadyLinkedException(provider);
        }

        _loginMethods.Add(AdminLoginMethod.CreateExternal(provider, providerUserId));
    }
}