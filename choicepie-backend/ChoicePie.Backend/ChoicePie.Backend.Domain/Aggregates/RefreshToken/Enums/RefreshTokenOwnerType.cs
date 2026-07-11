using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;

public sealed record RefreshTokenOwnerType(int Id, string Name) : Enumeration<RefreshTokenOwnerType>(Id, Name)
{
    public static readonly RefreshTokenOwnerType Member = new(1, "member");
    public static readonly RefreshTokenOwnerType Admin = new(2, "admin");
}
