using Application.DTOs.Reservations.Requests;
using Application.DTOs.Reservations.Responses;

namespace Application.Interfaces.Services.Reservations;

/// <summary>
/// Define la cancelación de reservas.
/// </summary>
public interface ICancelReservationService
{
    Task<ReservationResponse> ExecuteAsync(
        CancelReservationRequest request,
        CancellationToken cancellationToken = default);
}
