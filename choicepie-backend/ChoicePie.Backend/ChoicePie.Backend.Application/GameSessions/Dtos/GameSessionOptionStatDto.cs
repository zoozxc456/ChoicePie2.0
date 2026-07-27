namespace ChoicePie.Backend.Application.GameSessions.Dtos;

public sealed record GameSessionOptionStatDto(
    string Text,
    int PickedCount,
    bool IsCorrect);
