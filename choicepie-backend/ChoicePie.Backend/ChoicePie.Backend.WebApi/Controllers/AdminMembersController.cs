using Asp.Versioning;
using ChoicePie.Backend.Application.AdminMembers.Commands;
using ChoicePie.Backend.Application.AdminMembers.Dtos;
using ChoicePie.Backend.Application.AdminMembers.Queries;
using ChoicePie.Backend.Application.GameSessions.Dtos;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.WebApi.Requests.AdminMembers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChoicePie.Backend.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/members")]
[Authorize(Policy = "AdminOnly")]
public class AdminMembersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminMemberSummaryDto>>>> ListAsync([FromQuery] AdminListMembersQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminMemberDetailDto>>> GetAsync(Guid id)
    {
        var result = await mediator.Send(new AdminGetMemberByIdQuery(id));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet("{id:guid}/quizzes")]
    public async Task<ActionResult<ApiResponse<PagedResult<QuizSummaryDto>>>> ListQuizzesAsync(
        Guid id, [FromQuery] AdminListMemberQuizzesQuery query)
    {
        query.MemberId = id;
        var result = await mediator.Send(query);
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet("{id:guid}/hosted-sessions")]
    public async Task<ActionResult<ApiResponse<PagedResult<GameSessionSummaryDto>>>> ListHostedSessionsAsync(
        Guid id, [FromQuery] AdminListMemberHostedSessionsQuery query)
    {
        query.MemberId = id;
        var result = await mediator.Send(query);
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet("{id:guid}/played-sessions")]
    public async Task<ActionResult<ApiResponse<PagedResult<GameSessionSummaryDto>>>> ListPlayedSessionsAsync(
        Guid id, [FromQuery] AdminListMemberPlayedSessionsQuery query)
    {
        query.MemberId = id;
        var result = await mediator.Send(query);
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet("{id:guid}/comments")]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminMemberCommentDto>>>> ListCommentsAsync(
        Guid id, [FromQuery] AdminListMemberCommentsQuery query)
    {
        query.MemberId = id;
        var result = await mediator.Send(query);
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPost("{id:guid}/suspend")]
    public async Task<ActionResult<ApiResponse>> SuspendAsync(Guid id, [FromBody] SuspendMemberRequest request)
    {
        await mediator.Send(request.ToCommand(id));
        return Ok(ResponseHelper.Success());
    }

    [HttpPost("{id:guid}/unsuspend")]
    public async Task<ActionResult<ApiResponse>> UnsuspendAsync(Guid id)
    {
        await mediator.Send(new AdminUnsuspendMemberCommand(id));
        return Ok(ResponseHelper.Success());
    }
}
