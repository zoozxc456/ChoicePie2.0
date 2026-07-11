using Asp.Versioning;
using ChoicePie.Backend.Application.QuizAttempts.Commands;
using ChoicePie.Backend.Application.QuizAttempts.Queries;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.WebApi.Requests.QuizAttempts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChoicePie.Backend.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/quiz-attempts")]
[Authorize(Policy = "MemberOnly")]
public class QuizAttemptsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> StartAsync([FromBody] StartQuizAttemptRequest request)
    {
        var result = await mediator.Send(request.ToCommand());
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPost("{attemptId:guid}/answers")]
    public async Task<ActionResult> SubmitAnswerAsync(Guid attemptId, [FromBody] SubmitQuizAttemptAnswerRequest request)
    {
        await mediator.Send(request.ToCommand(attemptId));
        return Ok(ResponseHelper.Success());
    }

    [HttpPost("{attemptId:guid}/complete")]
    public async Task<ActionResult> CompleteAsync(Guid attemptId)
    {
        var result = await mediator.Send(new CompleteQuizAttemptCommand(attemptId));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet("{attemptId:guid}")]
    public async Task<ActionResult> GetByIdAsync(Guid attemptId)
    {
        var result = await mediator.Send(new GetQuizAttemptByIdQuery(attemptId));
        return Ok(ResponseHelper.Success(result));
    }
}
