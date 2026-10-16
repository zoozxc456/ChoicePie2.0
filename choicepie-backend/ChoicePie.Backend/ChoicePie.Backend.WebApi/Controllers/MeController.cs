using Asp.Versioning;
using ChoicePie.Backend.Application.CreatorFollows.Dtos;
using ChoicePie.Backend.Application.CreatorFollows.Queries;
using ChoicePie.Backend.Application.QuizFavorites.Queries;
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
[Route("api/v{version:apiVersion}/me")]
[Authorize(Policy = "MemberOnly")]
public class MeController(IMediator mediator) : ControllerBase
{
    [HttpGet("favorites")]
    public async Task<ActionResult<ApiResponse<PagedResult<QuizSummaryDto>>>> ListFavoritesAsync(
        [FromQuery] PagedRequest request)
    {
        var result = await mediator.Send(new ListMyFavoritesQuery(request.NormalizedPageNumber, request.NormalizedPageSize));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet("following")]
    public async Task<ActionResult<ApiResponse<PagedResult<FollowedCreatorDto>>>> ListFollowingAsync(
        [FromQuery] PagedRequest request)
    {
        var result = await mediator.Send(new ListMyFollowingQuery(request.NormalizedPageNumber, request.NormalizedPageSize));
        return Ok(ResponseHelper.Success(result));
    }

    [HttpGet("following/quizzes")]
    public async Task<ActionResult<ApiResponse<PagedResult<QuizSummaryDto>>>> ListFollowingQuizzesAsync(
        [FromQuery] PagedRequest request)
    {
        var result = await mediator.Send(new ListMyFollowingQuizzesQuery(request.NormalizedPageNumber, request.NormalizedPageSize));
        return Ok(ResponseHelper.Success(result));
    }
}
