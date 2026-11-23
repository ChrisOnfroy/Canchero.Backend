using Application.DTOs.Reservations.Requests;
using Application.DTOs.Reservations.Responses;

namespace Application.Interfaces.Services.Reservations;

/// <summary>
/// Define la creación de reservas.
/// </summary>
public interface ICreateReservationService
{
    Task<ReservationResponse> ExecuteAsync(
        CreateReservationRequest request,
        CancellationToken cancellationToken = default);
}
