using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.AdminUser.Enums;

public sealed record AdminRole(int Id, string Name) : Enumeration<AdminRole>(Id, Name)
{
    public static readonly AdminRole Admin = new(1, "admin");
    public static readonly AdminRole Staff = new(2, "staff");
    public static readonly AdminRole Maintainer = new(3, "maintainer");
    public static readonly AdminRole SystemAdmin = new(4, "systemAdmin");
}
