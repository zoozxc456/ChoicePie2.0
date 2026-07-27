using Asp.Versioning;
using ChoicePie.Backend.Application.AdminDashboard.Dtos;
using ChoicePie.Backend.Application.AdminDashboard.Queries;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChoicePie.Backend.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/dashboard")]
[Authorize(Policy = "AdminOnly")]
public class AdminDashboardController(IMediator mediator) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<AdminDashboardSummaryDto>>> GetSummaryAsync()
    {
        var result = await mediator.Send(new AdminGetDashboardSummaryQuery());
        return Ok(ResponseHelper.Success(result));
    }
}
