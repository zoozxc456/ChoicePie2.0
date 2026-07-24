using Asp.Versioning;
using ChoicePie.Backend.Application.Identity.Commands;
using ChoicePie.Backend.Application.Identity.Dtos;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Exceptions;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using ChoicePie.Backend.Shared.Kernel.Auth;
using ChoicePie.Backend.WebApi.Extensions;
using ChoicePie.Backend.WebApi.Requests.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ChoicePie.Backend.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController(IMediator mediator, IOptions<JwtSettings> jwtSettings) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<MemberDto>>> RegisterAsync([FromBody] RegisterMemberRequest request)
    {
        var result = await mediator.Send(request.ToCommand());
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResultDto>>> LoginAsync([FromBody] LoginRequest request)
    {
        var result = await mediator.Send(request.ToCommand());
        Response.SetAuthCookies(result.AccessToken, result.RefreshToken,
            jwtSettings.Value.AccessTokenExpirationSeconds);
        return Ok(ResponseHelper.Success(result.Member));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<MemberDto>>> RefreshAsync()
    {
        var refreshToken = Request.Cookies[AuthCookieNames.RefreshToken]
                           ?? throw new InvalidRefreshTokenException();

        var result = await mediator.Send(new RefreshTokenCommand { RefreshToken = refreshToken });
        Response.SetAuthCookies(result.AccessToken, result.RefreshToken,
            jwtSettings.Value.AccessTokenExpirationSeconds);
        return Ok(ResponseHelper.Success(result.Member));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse>> LogoutAsync()
    {
        if (Request.Cookies.TryGetValue(AuthCookieNames.RefreshToken, out var refreshToken))
        {
            await mediator.Send(new LogoutCommand { RefreshToken = refreshToken });
        }

        Response.ClearAuthCookies();
        return Ok(ResponseHelper.Success());
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse>> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request)
    {
        await mediator.Send(request.ToCommand());
        return Ok(ResponseHelper.Success());
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse>> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
    {
        await mediator.Send(request.ToCommand());
        return Ok(ResponseHelper.Success());
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<ApiResponse>> VerifyEmailAsync([FromBody] VerifyEmailRequest request)
    {
        await mediator.Send(request.ToCommand());
        return Ok(ResponseHelper.Success());
    }

    [HttpPost("resend-verification")]
    [Authorize(Policy = "MemberOnly")]
    public async Task<ActionResult<ApiResponse>> ResendVerificationAsync()
    {
        await mediator.Send(new ResendVerificationEmailCommand());
        return Ok(ResponseHelper.Success());
    }
}