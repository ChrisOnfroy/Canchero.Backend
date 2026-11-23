using Application.DTOs.Reservations.Requests;
using Application.DTOs.Reservations.Responses;

namespace Application.Interfaces.Services.Reservations;

/// <summary>
/// Define la finalización de reservas.
/// </summary>
public interface ICompleteReservationService
{
    Task<ReservationResponse> ExecuteAsync(
        CompleteReservationRequest request,
        CancellationToken cancellationToken = default);
}
