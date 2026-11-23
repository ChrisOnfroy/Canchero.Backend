using Application.DTOs.Reservations.Requests;
using Application.DTOs.Reservations.Responses;

namespace Application.Interfaces.Services.Reservations;

/// <summary>
/// Define el registro de inasistencias.
/// </summary>
public interface IMarkReservationAsNoShowService
{
    Task<ReservationResponse> ExecuteAsync(
        MarkReservationAsNoShowRequest request,
        CancellationToken cancellationToken = default);
}
