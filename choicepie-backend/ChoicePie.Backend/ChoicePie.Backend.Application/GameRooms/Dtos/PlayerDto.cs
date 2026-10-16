using ChoicePie.Backend.Domain.Aggregates.GameRoom.Entities;

namespace ChoicePie.Backend.Application.GameRooms.Dtos;

/// <summary>
/// 對玩家/大螢幕廣播時務必用 <see cref="ForPublic"/>：selectedOptionIndex 在公布答案前只給 Host。
/// 注意：絕不可在此加入 ReconnectToken——Id 會廣播給全房間，憑證只能回給玩家本人。
/// </summary>
public sealed record PlayerDto(
    Guid Id,
    string Nickname,
    int Score,
    int Rank,
    bool HasAnswered,
    int? SelectedOptionIndex,
    bool IsConnected = true)
{
    public static PlayerDto ForHost(Player player, int rank, int currentQuestionIndex) => new(
        player.Id,
        player.Nickname,
        player.Score,
        rank,
        player.HasAnsweredQuestion(currentQuestionIndex),
        player.SelectedOptionIndex,
        player.IsConnected);

    public static PlayerDto ForPublic(Player player, int rank, int currentQuestionIndex) => new(
        player.Id,
        player.Nickname,
        player.Score,
        rank,
        player.HasAnsweredQuestion(currentQuestionIndex),
        SelectedOptionIndex: null,
        player.IsConnected);
}
