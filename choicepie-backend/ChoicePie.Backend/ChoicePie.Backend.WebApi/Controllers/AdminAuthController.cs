using Asp.Versioning;
using ChoicePie.Backend.Application.AdminUsers.Commands;
using ChoicePie.Backend.Application.AdminUsers.Dtos;
using ChoicePie.Backend.Application.AdminUsers.Queries;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Exceptions;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using ChoicePie.Backend.Shared.Kernel.Auth;
using ChoicePie.Backend.WebApi.Extensions;
using ChoicePie.Backend.WebApi.Requests.AdminAuth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ChoicePie.Backend.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/auth")]
public class AdminAuthController(IMediator mediator, IOptions<JwtSettings> jwtSettings) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> LoginAsync([FromBody] AdminLoginRequest request)
    {
        var result = await mediator.Send(request.ToCommand());
        Response.SetAuthCookies(result.AccessToken, result.RefreshToken, jwtSettings.Value.AccessTokenExpirationSeconds);
        return Ok(ResponseHelper.Success(result.AdminUser));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> RefreshAsync()
    {
        var refreshToken = Request.Cookies[AuthCookieNames.RefreshToken]
                            ?? throw new InvalidRefreshTokenException();

        var result = await mediator.Send(new AdminRefreshTokenCommand { RefreshToken = refreshToken });
        Response.SetAuthCookies(result.AccessToken, result.RefreshToken, jwtSettings.Value.AccessTokenExpirationSeconds);
        return Ok(ResponseHelper.Success(result.AdminUser));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse>> LogoutAsync()
    {
        if (Request.Cookies.TryGetValue(AuthCookieNames.RefreshToken, out var refreshToken))
        {
            await mediator.Send(new AdminLogoutCommand { RefreshToken = refreshToken });
        }

        Response.ClearAuthCookies();
        return Ok(ResponseHelper.Success());
    }

    [HttpGet("me")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> GetCurrentAdminUserAsync()
    {
        var result = await mediator.Send(new GetCurrentAdminUserQuery());
        return Ok(ResponseHelper.Success(result));
    }
}
