using ChoicePie.Backend.Application.AdminDashboard.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.AdminDashboard.Queries;

public sealed record AdminGetDashboardSummaryQuery : IRequest<AdminDashboardSummaryDto>;
