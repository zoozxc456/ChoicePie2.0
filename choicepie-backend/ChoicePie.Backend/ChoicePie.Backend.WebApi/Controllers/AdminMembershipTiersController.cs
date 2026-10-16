using Asp.Versioning;
using ChoicePie.Backend.Application.MembershipTiers.Dtos;
using ChoicePie.Backend.Application.MembershipTiers.Queries;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.WebApi.Requests.AdminMembershipTiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChoicePie.Backend.WebApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin/membership-tiers")]
[Authorize(Policy = "AdminOnly")]
public class AdminMembershipTiersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MembershipTierDto>>>> ListAsync()
    {
        var result = await mediator.Send(new AdminListMembershipTiersQuery());
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MembershipTierDto>>> CreateAsync([FromBody] CreateMembershipTierRequest request)
    {
        var result = await mediator.Send(request.ToCommand());
        return Ok(ResponseHelper.Success(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<MembershipTierDto>>> UpdateAsync(Guid id, [FromBody] UpdateMembershipTierRequest request)
    {
        var result = await mediator.Send(request.ToCommand(id));
        return Ok(ResponseHelper.Success(result));
    }
}
