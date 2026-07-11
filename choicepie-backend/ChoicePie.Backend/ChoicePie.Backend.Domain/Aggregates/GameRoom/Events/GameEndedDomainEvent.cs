using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Events;

public sealed record GameEndedDomainEvent(
    Guid RoomId,
    string RoomCode,
    IReadOnlyList<RankEntrySnapshot> FinalRankings,
    DateTime EndedAtUtc) : BaseDomainEvent;
