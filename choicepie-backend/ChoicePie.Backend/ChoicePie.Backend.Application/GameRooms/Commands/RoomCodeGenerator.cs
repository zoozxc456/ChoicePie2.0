namespace ChoicePie.Backend.Application.GameRooms.Commands;

internal static class RoomCodeGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int Length = 6;

    public static string Generate() =>
        string.Create(Length, Random.Shared, static (span, random) =>
        {
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = Alphabet[random.Next(Alphabet.Length)];
            }
        });
}
