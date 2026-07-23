using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Enums;

public sealed record AdminLoginProvider(int Id, string Name) : Enumeration<AdminLoginProvider>(Id, Name)
{
    public static readonly AdminLoginProvider Original = new(1, "original");
}
