using Asp.Versioning;
using ChoicePie.Backend.Application.QuizFavorites.Commands;
using ChoicePie.Backend.Application.QuizFavorites.Queries;
using ChoicePie.Backend.Application.Quizzes.Commands;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Application.Quizzes.Queries;
using ChoicePie.Backend.Shared.Application.Contracts;
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
    public async Task<ActionResult<ApiResponse<QuizDto>>> CreateAsync([FromBody] CreateQuizRequest request)
    {
        var result = await mediator.Send(request.ToCommand());
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult<ApiResponse<QuizDto>>> UpdateAsync(Guid id, [FromBody] UpdateQuizRequest request)
    {
        var result = await mediator.Send(request.ToCommand(id));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult<ApiResponse>> DeleteAsync(Guid id)
    {
        await mediator.Send(new DeleteQuizCommand(id));
        return Ok(ResponseHelper.Success());
    }

    [HttpPost("{id:guid}/questions")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult<ApiResponse<QuizDto>>> AddQuestionAsync(Guid id, [FromBody] AddQuestionRequest request)
    {
        var result = await mediator.Send(request.ToCommand(id));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPut("{id:guid}/questions/{questionId:guid}")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult<ApiResponse<QuizDto>>> UpdateQuestionAsync(Guid id, Guid questionId, [FromBody] UpdateQuestionRequest request)
    {
        var result = await mediator.Send(request.ToCommand(id, questionId));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpDelete("{id:guid}/questions/{questionId:guid}")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult<ApiResponse<QuizDto>>> RemoveQuestionAsync(Guid id, Guid questionId)
    {
        var result = await mediator.Send(new RemoveQuestionCommand(id, questionId));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult<ApiResponse<QuizDto>>> PublishAsync(Guid id)
    {
        var result = await mediator.Send(new PublishQuizCommand(id));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPost("{id:guid}/unpublish")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult<ApiResponse<QuizDto>>> UnpublishAsync(Guid id)
    {
        var result = await mediator.Send(new UnpublishQuizCommand(id));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPost("{id:guid}/archive")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult<ApiResponse<QuizDto>>> ArchiveAsync(Guid id)
    {
        var result = await mediator.Send(new ArchiveQuizCommand(id));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPost("generate-questions")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult<ApiResponse<GenerateQuestionsResultDto>>> GenerateQuestionsAsync([FromBody] GenerateQuizQuestionsRequest request)
    {
        var result = await mediator.Send(request.ToCommand());
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult<ApiResponse<QuizDto>>> GetByIdAsync(Guid id)
    {
        var result = await mediator.Send(new GetQuizByIdQuery(id));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet("{id:guid}/preview")]
    public async Task<ActionResult<ApiResponse<QuizForAttemptDto>>> GetPreviewAsync(Guid id)
    {
        var result = await mediator.Send(new GetQuizForAttemptQuery(id));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<QuizSummaryDto>>>> ListAsync([FromQuery] ListQuizzesRequest request)
    {
        var ownerId = request.Mine ? currentUserService.UserId : null;
        var result = await mediator.Send(request.ToQuery(ownerId));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet("tags")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<string>>>> GetTagsAsync()
    {
        var result = await mediator.Send(new GetQuizTagsQuery());
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet("{id:guid}/favorite")]
    public async Task<ActionResult<ApiResponse<bool>>> GetFavoriteStatusAsync(Guid id)
    {
        var result = await mediator.Send(new GetQuizFavoriteStatusQuery(id));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPut("{id:guid}/favorite")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult<ApiResponse>> AddFavoriteAsync(Guid id)
    {
        await mediator.Send(new AddQuizFavoriteCommand(id));
        return Ok(ResponseHelper.Success());
    }

    [HttpDelete("{id:guid}/favorite")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult<ApiResponse>> RemoveFavoriteAsync(Guid id)
    {
        await mediator.Send(new RemoveQuizFavoriteCommand(id));
        return Ok(ResponseHelper.Success());
    }
}
