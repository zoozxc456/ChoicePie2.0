using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;

namespace ChoicePie.Backend.Application.GameRooms.Dtos;

public sealed record RankEntryDto(int Rank, string Nickname, int Score)
{
    public static RankEntryDto FromDomain(RankEntrySnapshot snapshot) =>
        new(snapshot.Rank, snapshot.Nickname, snapshot.Score);
}
