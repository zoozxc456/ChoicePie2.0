using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;

public sealed record RankEntrySnapshot(Guid PlayerId, string Nickname, int Score, int Rank) : ValueObject;
