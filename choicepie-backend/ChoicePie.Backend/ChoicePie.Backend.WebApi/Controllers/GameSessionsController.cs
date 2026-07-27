using Asp.Versioning;
using ChoicePie.Backend.Application.GameSessions.Dtos;
using ChoicePie.Backend.Application.GameSessions.Queries;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChoicePie.Backend.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/game-sessions")]
[Authorize(Policy = "MemberOnly")]
public class GameSessionsController(IMediator mediator) : ControllerBase
{
    [HttpGet("hosted")]
    public async Task<ActionResult<ApiResponse<PagedResult<GameSessionSummaryDto>>>> ListHostedAsync(
        [FromQuery(Name = "page")] int pageNumber = 1, [FromQuery(Name = "pageSize")] int pageSize = 20)
    {
        var result = await mediator.Send(new GetHostedGameSessionsQuery(pageNumber, pageSize));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet("played")]
    public async Task<ActionResult<ApiResponse<PagedResult<GameSessionSummaryDto>>>> ListPlayedAsync(
        [FromQuery(Name = "page")] int pageNumber = 1, [FromQuery(Name = "pageSize")] int pageSize = 20)
    {
        var result = await mediator.Send(new GetPlayedGameSessionsQuery(pageNumber, pageSize));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<GameSessionDetailDto>>> GetByIdAsync(Guid id)
    {
        var result = await mediator.Send(new GetGameSessionByIdQuery(id));
        return Ok(ResponseHelper.Success(result));
    }
}
