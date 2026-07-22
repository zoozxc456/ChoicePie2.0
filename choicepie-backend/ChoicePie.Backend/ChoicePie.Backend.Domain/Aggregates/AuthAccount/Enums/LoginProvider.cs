using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;

public sealed record LoginProvider(int Id, string Name) : Enumeration<LoginProvider>(Id, Name)
{
    public static readonly LoginProvider Original = new(1, "original");
    public static readonly LoginProvider Google = new(2, "google");
    public static readonly LoginProvider Meta = new(3, "meta");
    public static readonly LoginProvider Line = new(4, "line");
}
