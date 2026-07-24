using Asp.Versioning;
using ChoicePie.Backend.Application.AdminQuizzes.Commands;
using ChoicePie.Backend.Application.AdminQuizzes.Queries;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.WebApi.Requests.AdminQuizzes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChoicePie.Backend.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/quizzes")]
[Authorize(Policy = "AdminOnly")]
public class AdminQuizzesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<QuizSummaryDto>>>> ListAsync([FromQuery] AdminListQuizzesQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPost("{id:guid}/takedown")]
    public async Task<ActionResult<ApiResponse>> TakeDownAsync(Guid id, [FromBody] TakeDownQuizRequest request)
    {
        await mediator.Send(request.ToCommand(id));
        return Ok(ResponseHelper.Success());
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<ActionResult<ApiResponse>> RestoreAsync(Guid id)
    {
        await mediator.Send(new AdminRestoreQuizCommand(id));
        return Ok(ResponseHelper.Success());
    }
}
