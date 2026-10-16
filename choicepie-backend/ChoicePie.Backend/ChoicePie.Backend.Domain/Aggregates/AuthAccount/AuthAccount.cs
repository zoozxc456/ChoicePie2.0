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

    public HashedPassword? OriginalPassword =>
        _loginMethods.SingleOrDefault(m => m.Provider == LoginProvider.Original)?.Password;

    public bool HasPassword => OriginalPassword is not null;

    private AuthAccount()
    {
    }

    public static AuthAccount Register(Email email, HashedPassword password, Guid memberId)
    {
        var authAccount = new AuthAccount
        {
            Id = Guid.NewGuid(),
            Email = email,
            MemberId = memberId,
            IsVerified = false
        };

        authAccount.SetCreated(authAccount.Id);
        authAccount._loginMethods.Add(LoginMethod.CreateOriginal(password));
        authAccount.AddDomainEvent(new AuthAccountRegisteredDomainEvent(authAccount.Id, memberId, email.Value));

        return authAccount;
    }

    public static AuthAccount RegisterWithExternalLogin(
        Email email, LoginProvider provider, string providerUserId, Guid memberId)
    {
        var authAccount = new AuthAccount
        {
            Id = Guid.NewGuid(),
            Email = email,
            MemberId = memberId,
            IsVerified = true
        };

        authAccount.SetCreated(authAccount.Id);
        authAccount._loginMethods.Add(LoginMethod.CreateExternal(provider, providerUserId));
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

    public void ChangePassword(HashedPassword password)
    {
        var originalLoginMethod = _loginMethods.SingleOrDefault(m => m.Provider == LoginProvider.Original)
                                   ?? throw new NoOriginalLoginMethodException(Id);

        originalLoginMethod.SetPassword(password);
        Touch();
    }

    public bool IsLinkedTo(LoginProvider provider) => _loginMethods.Any(m => m.Provider == provider);

    // 為純第三方登入（例如只有 Google）的帳號補上密碼：新增 Original 登入方式。
    public void AddPassword(HashedPassword password)
    {
        if (HasPassword)
        {
            throw new PasswordAlreadySetException(Id);
        }

        _loginMethods.Add(LoginMethod.CreateOriginal(password));
        Touch();
    }

    // 只能解除第三方登入；密碼（Original）只能被取代，不能被移除。
    // 不變條件：帳號至少要保留一種登入方式，否則使用者會被鎖在帳號外。
    public void RemoveLoginMethod(LoginProvider provider)
    {
        var loginMethod = provider == LoginProvider.Original
            ? null
            : _loginMethods.SingleOrDefault(m => m.Provider == provider);

        if (loginMethod is null)
        {
            throw new LoginMethodNotLinkedException(provider);
        }

        if (_loginMethods.Count <= 1)
        {
            throw new CannotRemoveLastLoginMethodException(Id);
        }

        _loginMethods.Remove(loginMethod);
        Touch();
    }

    // 軟刪除帳號。Email 上有不分刪除狀態的唯一索引，所以把 Email 改成不可寄送的墓碑值，
    // 讓同一個 email 之後可以重新註冊成全新的帳號；同時清掉密碼雜湊與第三方身分（個資最小化）。
    public void MarkDeleted(Guid deleterId)
    {
        Email = Email.Create($"deleted-{Id:N}@deleted.invalid");
        _loginMethods.Clear();
        Delete(deleterId);
    }

    public void Verify()
    {
        if (IsVerified)
        {
            return;
        }

        IsVerified = true;
        Touch();
    }
}