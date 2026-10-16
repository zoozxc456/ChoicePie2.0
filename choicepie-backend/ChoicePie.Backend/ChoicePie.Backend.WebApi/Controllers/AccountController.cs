using Asp.Versioning;
using ChoicePie.Backend.Application.Account.Dtos;
using ChoicePie.Backend.Application.Account.Commands;
using ChoicePie.Backend.Application.Account.Queries;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.Shared.Hosting.RateLimiting;
using ChoicePie.Backend.Shared.Kernel.Auth;
using ChoicePie.Backend.WebApi.Extensions;
using ChoicePie.Backend.WebApi.Requests.Account;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ChoicePie.Backend.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/account")]
[Authorize(Policy = "MemberOnly")]
public class AccountController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<AccountDto>>> GetAsync()
    {
        var result = await mediator.Send(new GetAccountQuery());
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPut("profile")]
    [EnableRateLimiting(RateLimitPolicyNames.Write)]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> UpdateProfileAsync([FromBody] UpdateProfileRequest request)
    {
        var result = await mediator.Send(request.ToCommand());
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPost("avatar")]
    [EnableRateLimiting(RateLimitPolicyNames.Write)]
    [RequestSizeLimit(5_000_000)]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> UploadAvatarAsync([FromForm] UploadAvatarRequest request)
    {
        var result = await mediator.Send(request.ToCommand());
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPut("password")]
    [EnableRateLimiting(RateLimitPolicyNames.Auth)]
    public async Task<ActionResult<ApiResponse>> ChangePasswordAsync([FromBody] ChangePasswordRequest request)
    {
        // 目前 session 的 refresh token：變更密碼時保留它，其餘裝置的 session 全部撤銷。
        var currentRefreshToken = Request.Headers[AuthHeaderNames.RefreshToken].FirstOrDefault();
        await mediator.Send(request.ToCommand(currentRefreshToken));
        return Ok(ResponseHelper.Success());
    }

    [HttpPost("google")]
    [EnableRateLimiting(RateLimitPolicyNames.Auth)]
    public async Task<ActionResult<ApiResponse>> LinkGoogleAsync([FromBody] LinkGoogleRequest request)
    {
        await mediator.Send(request.ToCommand());
        return Ok(ResponseHelper.Success());
    }

    [HttpDelete("google")]
    [EnableRateLimiting(RateLimitPolicyNames.Auth)]
    public async Task<ActionResult<ApiResponse>> UnlinkGoogleAsync()
    {
        await mediator.Send(new UnlinkGoogleCommand());
        return Ok(ResponseHelper.Success());
    }

    [HttpDelete]
    [EnableRateLimiting(RateLimitPolicyNames.Auth)]
    public async Task<ActionResult<ApiResponse>> DeleteAsync([FromBody] DeleteAccountRequest request)
    {
        await mediator.Send(request.ToCommand());
        Response.ClearAuthCookies();
        return Ok(ResponseHelper.Success());
    }
}
