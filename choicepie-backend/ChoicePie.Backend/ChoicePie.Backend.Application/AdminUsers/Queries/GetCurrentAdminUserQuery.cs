using ChoicePie.Backend.Application.AdminUsers.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.AdminUsers.Queries;

public sealed class GetCurrentAdminUserQuery : IRequest<AdminUserDto>;
