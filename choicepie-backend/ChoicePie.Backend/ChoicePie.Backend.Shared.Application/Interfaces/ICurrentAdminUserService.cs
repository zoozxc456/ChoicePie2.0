namespace ChoicePie.Backend.Shared.Application.Interfaces;

public interface ICurrentAdminUserService
{
    Guid? AdminUserId { get; }
}
