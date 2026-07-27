using Asp.Versioning;
using ChoicePie.Backend.Application.AdminQuizReports.Queries;
using ChoicePie.Backend.Application.QuizReports.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.WebApi.Requests.AdminQuizReports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChoicePie.Backend.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/quiz-reports")]
[Authorize(Policy = "AdminOnly")]
public class AdminQuizReportsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<QuizReportDto>>>> ListAsync([FromQuery] AdminListQuizReportsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPost("{id:guid}/resolve")]
    public async Task<ActionResult<ApiResponse>> ResolveAsync(Guid id, [FromBody] ResolveQuizReportRequest request)
    {
        await mediator.Send(request.ToCommand(id));
        return Ok(ResponseHelper.Success());
    }

    [HttpPost("{id:guid}/dismiss")]
    public async Task<ActionResult<ApiResponse>> DismissAsync(Guid id, [FromBody] DismissQuizReportRequest request)
    {
        await mediator.Send(request.ToCommand(id));
        return Ok(ResponseHelper.Success());
    }
}
