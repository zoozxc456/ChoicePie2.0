using ChoicePie.Backend.Application.AdminQuizzes.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.AdminQuizzes.Queries;

public sealed record AdminGetQuizByIdQuery(Guid Id) : IRequest<AdminQuizDetailDto>;
