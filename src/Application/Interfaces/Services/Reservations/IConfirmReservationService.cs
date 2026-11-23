using Application.DTOs.Reservations.Requests;
using Application.DTOs.Reservations.Responses;

namespace Application.Interfaces.Services.Reservations;

/// <summary>
/// Define la confirmación de reservas.
/// </summary>
public interface IConfirmReservationService
{
    Task<ReservationResponse> ExecuteAsync(
        ConfirmReservationRequest request,
        CancellationToken cancellationToken = default);
}
