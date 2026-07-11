using Asp.Versioning;
using ChoicePie.Backend.Application.Identity.Commands;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Exceptions;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using ChoicePie.Backend.Shared.Kernel.Auth;
using ChoicePie.Backend.WebApi.Extensions;
using ChoicePie.Backend.WebApi.Requests.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ChoicePie.Backend.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController(IMediator mediator, IOptions<JwtSettings> jwtSettings) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult> RegisterAsync([FromBody] RegisterMemberRequest request)
    {
        var result = await mediator.Send(request.ToCommand());
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPost("login")]
    public async Task<ActionResult> LoginAsync([FromBody] LoginRequest request)
    {
        var result = await mediator.Send(request.ToCommand());
        Response.SetAuthCookies(result.AccessToken, result.RefreshToken, jwtSettings.Value.AccessTokenExpirationSeconds);
        return Ok(ResponseHelper.Success(result.Member));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult> RefreshAsync()
    {
        var refreshToken = Request.Cookies[AuthCookieNames.RefreshToken]
                            ?? throw new InvalidRefreshTokenException();

        var result = await mediator.Send(new RefreshTokenCommand { RefreshToken = refreshToken });
        Response.SetAuthCookies(result.AccessToken, result.RefreshToken, jwtSettings.Value.AccessTokenExpirationSeconds);
        return Ok(ResponseHelper.Success(result.Member));
    }

    [HttpPost("logout")]
    public async Task<ActionResult> LogoutAsync()
    {
        if (Request.Cookies.TryGetValue(AuthCookieNames.RefreshToken, out var refreshToken))
        {
            await mediator.Send(new LogoutCommand { RefreshToken = refreshToken });
        }

        Response.ClearAuthCookies();
        return Ok(ResponseHelper.Success());
    }
}
