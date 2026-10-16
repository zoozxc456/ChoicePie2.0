using ChoicePie.Backend.Shared.Hosting.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Asp.Versioning;
using ChoicePie.Backend.Application.CreatorFollows.Commands;
using ChoicePie.Backend.Application.CreatorFollows.Dtos;
using ChoicePie.Backend.Application.CreatorFollows.Queries;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.WebApi.Requests.Me;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChoicePie.Backend.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/creators")]
public class CreatorsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CreatorProfileDto>>> GetByIdAsync(Guid id)
    {
        var result = await mediator.Send(new GetCreatorProfileQuery(id));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet("{id:guid}/quizzes")]
    public async Task<ActionResult<ApiResponse<PagedResult<QuizSummaryDto>>>> ListQuizzesAsync(
        Guid id, [FromQuery] PagedRequest request)
    {
        var result = await mediator.Send(
            new ListCreatorQuizzesQuery(id, request.NormalizedPageNumber, request.NormalizedPageSize));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPut("{id:guid}/follow")]
    [EnableRateLimiting(RateLimitPolicyNames.Write)]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult<ApiResponse>> FollowAsync(Guid id)
    {
        await mediator.Send(new AddCreatorFollowCommand(id));
        return Ok(ResponseHelper.Success());
    }

    [HttpDelete("{id:guid}/follow")]
    [EnableRateLimiting(RateLimitPolicyNames.Write)]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult<ApiResponse>> UnfollowAsync(Guid id)
    {
        await mediator.Send(new RemoveCreatorFollowCommand(id));
        return Ok(ResponseHelper.Success());
    }
}
