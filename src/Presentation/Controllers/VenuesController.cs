using System.Security.Claims;
using Application.DTOs.Venues.Requests;
using Application.DTOs.Venues.Responses;
using Application.Interfaces.Services.Venues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api/venues")]
[Authorize]
public sealed class VenuesController(
    ICreateVenueService create, IUpdateVenueService update, IChangeVenueStatusService changeStatus,
    IGetVenueByIdService get, IListVenuesByOwnerService list) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<VenueResponse>> Create(CreateVenueRequest request, CancellationToken ct) =>
        Ok(await create.ExecuteAsync(request with { ActorUserId = ActorId() }, ct));
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VenueResponse>> Update(Guid id, UpdateVenueRequest request, CancellationToken ct) =>
        Ok(await update.ExecuteAsync(request with { ActorUserId = ActorId(), VenueId = id }, ct));
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<VenueResponse>> ChangeStatus(Guid id, bool isActive, CancellationToken ct) =>
        Ok(await changeStatus.ExecuteAsync(new ChangeVenueStatusRequest(ActorId(), id, isActive), ct));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VenueResponse>> Get(Guid id, CancellationToken ct) =>
        Ok(await get.ExecuteAsync(new GetVenueByIdRequest(ActorId(), id), ct));
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VenueResponse>>> List([FromQuery] Guid? ownerId, CancellationToken ct)
    {
        var actor = ActorId();
        return Ok(await list.ExecuteAsync(new ListVenuesByOwnerRequest(actor, ownerId ?? actor), ct));
    }
    private Guid ActorId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
