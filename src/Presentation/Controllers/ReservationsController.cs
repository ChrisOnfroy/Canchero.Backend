using System.Security.Claims;
using Application.Common;
using Application.DTOs.Reservations.Requests;
using Application.DTOs.Reservations.Responses;
using Application.Interfaces.Services.Reservations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public sealed class ReservationsController(
    ICreateReservationService create, ICancelReservationService cancel, IConfirmReservationService confirm,
    ICompleteReservationService complete, IMarkReservationAsNoShowService noShow,
    IListUserReservationsService userList, IListCourtReservationsService courtList,
    IGetCourtAvailabilityService availability) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ReservationResponse>> Create(CreateReservationRequest request, CancellationToken ct) =>
        Ok(await create.ExecuteAsync(request with { ActorUserId = ActorId() }, ct));
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ReservationResponse>> Cancel(Guid id, [FromBody] CancelBody? body, CancellationToken ct) =>
        Ok(await cancel.ExecuteAsync(new CancelReservationRequest(ActorId(), id, body?.Reason), ct));
    [HttpPost("{id:guid}/confirm")]
    public async Task<ActionResult<ReservationResponse>> Confirm(Guid id, CancellationToken ct) =>
        Ok(await confirm.ExecuteAsync(new ConfirmReservationRequest(ActorId(), id), ct));
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ReservationResponse>> Complete(Guid id, CancellationToken ct) =>
        Ok(await complete.ExecuteAsync(new CompleteReservationRequest(ActorId(), id), ct));
    [HttpPost("{id:guid}/no-show")]
    public async Task<ActionResult<ReservationResponse>> NoShow(Guid id, CancellationToken ct) =>
        Ok(await noShow.ExecuteAsync(new MarkReservationAsNoShowRequest(ActorId(), id), ct));
    [HttpGet("mine")]
    public async Task<ActionResult<PagedResult<ReservationResponse>>> Mine([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] Domain.Enums.ReservationStatus? status = null, CancellationToken ct = default)
    {
        var actor = ActorId();
        return Ok(await userList.ExecuteAsync(new ListUserReservationsRequest(actor, actor, page, pageSize, status), ct));
    }
    [HttpGet("court/{courtId:guid}")]
    public async Task<ActionResult<PagedResult<ReservationResponse>>> ByCourt(Guid courtId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default) =>
        Ok(await courtList.ExecuteAsync(new ListCourtReservationsRequest(ActorId(), courtId, page, pageSize), ct));
    [HttpGet("court/{courtId:guid}/availability")]
    public async Task<ActionResult<GetCourtAvailabilityResponse>> Availability(Guid courtId, [FromQuery] DateTimeOffset day, CancellationToken ct) =>
        Ok(await availability.ExecuteAsync(new GetCourtAvailabilityRequest(ActorId(), courtId, day), ct));
    private Guid ActorId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    public sealed record CancelBody(string? Reason);
}
