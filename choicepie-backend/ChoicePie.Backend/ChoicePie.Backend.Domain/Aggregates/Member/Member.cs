using ChoicePie.Backend.Domain.Aggregates.Member.Events;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;

namespace ChoicePie.Backend.Domain.Aggregates.Member;

public sealed class Member : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    public string? Avatar { get; private set; }
    public DateTime? LastAiGenerationAt { get; private set; }
    public bool IsSuspended { get; private set; }
    public string? SuspendedReason { get; private set; }
    public DateTime? SuspendedUntil { get; private set; }
    public Guid? TierId { get; private set; }

    private Member()
    {
    }

    public static Member Create(string name)
    {
        var personName = PersonName.Create(name, n => new InvalidMemberNameException(n));

        var member = new Member
        {
            Id = Guid.NewGuid(),
            Name = personName.Value
        };

        member.SetCreated(member.Id);
        member.AddDomainEvent(new MemberCreatedDomainEvent(member.Id, member.Name));

        return member;
    }

    public void Rename(string name)
    {
        var personName = PersonName.Create(name, n => new InvalidMemberNameException(n));

        Name = personName.Value.Trim();
        Touch();
        AddDomainEvent(new MemberProfileChangedDomainEvent(Id));
    }

    // 回傳舊的頭像網址，讓呼叫端可以清掉不再使用的檔案。
    public string? ChangeAvatar(string avatarUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(avatarUrl);

        var previous = Avatar;
        Avatar = avatarUrl;
        Touch();
        AddDomainEvent(new MemberProfileChangedDomainEvent(Id));

        return previous;
    }

    // 使用者自行關閉（軟刪除）帳號；其他聚合（例如該使用者已發布的題庫）透過事件反應。
    public void CloseAccount()
    {
        Delete(Id);
        AddDomainEvent(new MemberAccountClosedDomainEvent(Id));
    }

    public void RecordAiGeneration(DateTime nowUtc)
    {
        LastAiGenerationAt = nowUtc;
    }

    public void AssignTier(Guid tierId)
    {
        TierId = tierId;
    }

    // SuspendedUntil = null means a permanent suspension; a concrete date expires automatically
    // (checked in IsCurrentlySuspended) without any background job clearing IsSuspended.
    public void Suspend(string reason, DateTime? until)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidMemberSuspensionException("停權原因不能為空。");
        }

        IsSuspended = true;
        SuspendedReason = reason;
        SuspendedUntil = until;
        AddDomainEvent(new MemberSuspendedDomainEvent(Id));
    }

    public void Unsuspend()
    {
        IsSuspended = false;
        SuspendedReason = null;
        SuspendedUntil = null;
        AddDomainEvent(new MemberUnsuspendedDomainEvent(Id));
    }

    public bool IsCurrentlySuspended(DateTime nowUtc)
    {
        if (!IsSuspended)
        {
            return false;
        }

        return SuspendedUntil is null || SuspendedUntil.Value > nowUtc;
    }
}
