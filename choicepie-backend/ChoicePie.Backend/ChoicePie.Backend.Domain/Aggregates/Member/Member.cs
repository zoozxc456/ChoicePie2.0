using ChoicePie.Backend.Domain.Aggregates.Member.Events;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;

namespace ChoicePie.Backend.Domain.Aggregates.Member;

public sealed class Member : AggregateRoot<Guid>
{
    private const int MinNameLength = 2;
    private const int MaxNameLength = 20;

    public Email Email { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Avatar { get; private set; }
    public string PasswordHash { get; private set; } = null!;
    public bool IsVerified { get; private set; }
    public DateTime? LastAiGenerationAt { get; private set; }

    private Member()
    {
    }

    public static Member Register(Email email, string name, string passwordHash)
    {
        ValidateName(name);

        var member = new Member
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = name,
            PasswordHash = passwordHash,
            IsVerified = false
        };

        member.SetCreated(member.Id);
        member.AddDomainEvent(new MemberRegisteredDomainEvent(member.Id, member.Email.Value, member.Name));

        return member;
    }

    public bool CanGenerateQuizToday(DateTime nowUtc)
    {
        return LastAiGenerationAt is null || LastAiGenerationAt.Value.Date < nowUtc.Date;
    }

    public void RecordAiGeneration(DateTime nowUtc)
    {
        LastAiGenerationAt = nowUtc;
    }

    private static void ValidateName(string name)
    {
        var trimmed = name.Trim();

        if (trimmed.Length is < MinNameLength or > MaxNameLength)
        {
            throw new InvalidMemberNameException(name);
        }
    }
}
