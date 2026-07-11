namespace ChoicePie.Backend.Application.AdminUsers.Dtos;

public sealed record AdminLoginResultDto(AdminUserDto AdminUser, string AccessToken, string RefreshToken);
