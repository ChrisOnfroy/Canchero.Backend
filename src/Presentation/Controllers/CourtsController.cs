using System.Security.Claims;
using Application.DTOs.Courts.Requests;
using Application.DTOs.Courts.Responses;
using Application.Interfaces.Services.Courts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Presentation.Controllers;

[ApiController]
[Route("api/courts")]
[Authorize]
public sealed class CourtsController(
    ICreateCourtService create, IUpdateCourtService update, IChangeCourtStatusService changeStatus,
    IGetCourtByIdService get, ISearchNearbyCourtsService search) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CourtResponse>> Create(CreateCourtRequest request, CancellationToken ct) =>
        Ok(await create.ExecuteAsync(request with { ActorUserId = ActorId() }, ct));
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CourtResponse>> Update(Guid id, UpdateCourtRequest request, CancellationToken ct) =>
        Ok(await update.ExecuteAsync(request with { ActorUserId = ActorId(), CourtId = id }, ct));
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<CourtResponse>> ChangeStatus(Guid id, [FromBody] ChangeStatusBody body, CancellationToken ct) =>
        Ok(await changeStatus.ExecuteAsync(new ChangeCourtStatusRequest(ActorId(), id, body.Status), ct));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourtResponse>> Get(Guid id, CancellationToken ct) =>
        Ok(await get.ExecuteAsync(new GetCourtByIdRequest(ActorId(), id), ct));
    [HttpGet("search")]
    [EnableRateLimiting("search")]
    public async Task<ActionResult<SearchNearbyCourtsResponse>> Search([FromQuery] SearchNearbyCourtsRequest request, CancellationToken ct) =>
        Ok(await search.ExecuteAsync(request with { ActorUserId = ActorId() }, ct));
    private Guid ActorId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    public sealed record ChangeStatusBody(Domain.Enums.CourtStatus Status);
}
