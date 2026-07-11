using Asp.Versioning;
using ChoicePie.Backend.Application.Quizzes.Commands;
using ChoicePie.Backend.Application.Quizzes.Queries;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.WebApi.Requests.Quizzes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChoicePie.Backend.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/quizzes")]
public class QuizzesController(IMediator mediator, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult> CreateAsync([FromBody] CreateQuizRequest request)
    {
        var result = await mediator.Send(request.ToCommand());
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult> UpdateAsync(Guid id, [FromBody] UpdateQuizRequest request)
    {
        var result = await mediator.Send(request.ToCommand(id));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult> DeleteAsync(Guid id)
    {
        await mediator.Send(new DeleteQuizCommand(id));
        return Ok(ResponseHelper.Success());
    }

    [HttpPost("{id:guid}/questions")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult> AddQuestionAsync(Guid id, [FromBody] AddQuestionRequest request)
    {
        var result = await mediator.Send(request.ToCommand(id));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPut("{id:guid}/questions/{questionId:guid}")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult> UpdateQuestionAsync(Guid id, Guid questionId, [FromBody] UpdateQuestionRequest request)
    {
        var result = await mediator.Send(request.ToCommand(id, questionId));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpDelete("{id:guid}/questions/{questionId:guid}")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult> RemoveQuestionAsync(Guid id, Guid questionId)
    {
        var result = await mediator.Send(new RemoveQuestionCommand(id, questionId));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult> PublishAsync(Guid id)
    {
        var result = await mediator.Send(new PublishQuizCommand(id));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPost("{id:guid}/unpublish")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult> UnpublishAsync(Guid id)
    {
        var result = await mediator.Send(new UnpublishQuizCommand(id));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPost("{id:guid}/archive")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult> ArchiveAsync(Guid id)
    {
        var result = await mediator.Send(new ArchiveQuizCommand(id));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPost("generate-questions")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult> GenerateQuestionsAsync([FromBody] GenerateQuizQuestionsRequest request)
    {
        var result = await mediator.Send(request.ToCommand());
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult> GetByIdAsync(Guid id)
    {
        var result = await mediator.Send(new GetQuizByIdQuery(id));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet("{id:guid}/preview")]
    public async Task<ActionResult> GetPreviewAsync(Guid id)
    {
        var result = await mediator.Send(new GetQuizForAttemptQuery(id));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet]
    public async Task<ActionResult> ListAsync(
        [FromQuery] string? tag, [FromQuery] string? search, [FromQuery] bool mine,
        [FromQuery(Name = "page")] int pageNumber = 1, [FromQuery(Name = "pageSize")] int pageSize = 20)
    {
        var ownerId = mine ? currentUserService.UserId : null;
        var query = new ListQuizzesQuery
        {
            Tag = tag, Search = search, OwnerId = ownerId, PageNumber = pageNumber, PageSize = pageSize
        };
        var result = await mediator.Send(query);
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet("tags")]
    public async Task<ActionResult> GetTagsAsync()
    {
        var result = await mediator.Send(new GetQuizTagsQuery());
        return Ok(ResponseHelper.Success(result));
    }
}
