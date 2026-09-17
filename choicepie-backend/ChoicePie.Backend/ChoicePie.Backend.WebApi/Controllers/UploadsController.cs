using Asp.Versioning;
using ChoicePie.Backend.Application.Uploads.Dtos;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.WebApi.Requests.Uploads;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChoicePie.Backend.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/uploads")]
public class UploadsController(IMediator mediator) : ControllerBase
{
    [HttpPost("quiz-covers")]
    [Authorize(Policy = "MemberOnly")]
    [RequestSizeLimit(5_000_000)]
    public async Task<ActionResult<ApiResponse<UploadQuizCoverResultDto>>> UploadQuizCoverAsync(
        [FromForm] UploadQuizCoverRequest request)
    {
        var result = await mediator.Send(request.ToCommand());
        return Ok(ResponseHelper.Success(result));
    }
}
