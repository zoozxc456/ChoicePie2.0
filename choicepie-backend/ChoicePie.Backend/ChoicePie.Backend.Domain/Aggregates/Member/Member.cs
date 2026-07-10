using ChoicePie.Backend.Domain.Aggregates.Member.Events;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.Member;

public sealed class Member : AggregateRoot<Guid>
{
    private const int MinNameLength = 2;
    private const int MaxNameLength = 20;

    public string Name { get; private set; } = null!;
    public string? Avatar { get; private set; }
    public DateTime? LastAiGenerationAt { get; private set; }

    private Member()
    {
    }

    public static Member Create(string name)
    {
        ValidateName(name);

        var member = new Member
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        member.SetCreated(member.Id);
        member.AddDomainEvent(new MemberCreatedDomainEvent(member.Id, member.Name));

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
