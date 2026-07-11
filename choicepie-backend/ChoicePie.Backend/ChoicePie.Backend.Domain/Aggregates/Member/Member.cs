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

    public bool CanGenerateQuizToday(DateTime nowUtc)
    {
        return LastAiGenerationAt is null || LastAiGenerationAt.Value.Date < nowUtc.Date;
    }

    public void RecordAiGeneration(DateTime nowUtc)
    {
        LastAiGenerationAt = nowUtc;
    }
}
