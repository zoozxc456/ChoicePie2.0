using Asp.Versioning;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.WebApi.Requests.AdminAuth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChoicePie.Backend.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/auth")]
public class AdminAuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult> LoginAsync([FromBody] AdminLoginRequest request)
    {
        var result = await mediator.Send(request.ToCommand());
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult> RefreshAsync([FromBody] AdminRefreshTokenRequest request)
    {
        var result = await mediator.Send(request.ToCommand());
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPost("logout")]
    public async Task<ActionResult> LogoutAsync([FromBody] AdminLogoutRequest request)
    {
        await mediator.Send(request.ToCommand());
        return Ok(ResponseHelper.Success());
    }
}
