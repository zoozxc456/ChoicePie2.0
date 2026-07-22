using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Enums;

public sealed record GamePhase(int Id, string Name) : Enumeration<GamePhase>(Id, Name)
{
    public static readonly GamePhase Lobby = new(1, "lobby");
    public static readonly GamePhase Question = new(2, "question");
    public static readonly GamePhase Reveal = new(3, "reveal");
    public static readonly GamePhase Ended = new(4, "ended");
}
