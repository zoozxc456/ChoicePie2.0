using Asp.Versioning;
using ChoicePie.Backend.Application.CreatorFollows.Commands;
using ChoicePie.Backend.Application.CreatorFollows.Dtos;
using ChoicePie.Backend.Application.CreatorFollows.Queries;
using ChoicePie.Backend.Shared.Hosting.API.Response;
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

    [HttpPut("{id:guid}/follow")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult<ApiResponse>> FollowAsync(Guid id)
    {
        await mediator.Send(new AddCreatorFollowCommand(id));
        return Ok(ResponseHelper.Success());
    }

    [HttpDelete("{id:guid}/follow")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult<ApiResponse>> UnfollowAsync(Guid id)
    {
        await mediator.Send(new RemoveCreatorFollowCommand(id));
        return Ok(ResponseHelper.Success());
    }
}
