namespace ChoicePie.Backend.Shared.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}
