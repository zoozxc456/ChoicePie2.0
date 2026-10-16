using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.Member.Events;

// 名稱或頭像改變；其他地方顯示的創作者資訊需要據此更新。
public sealed record MemberProfileChangedDomainEvent(Guid MemberId) : BaseDomainEvent;
