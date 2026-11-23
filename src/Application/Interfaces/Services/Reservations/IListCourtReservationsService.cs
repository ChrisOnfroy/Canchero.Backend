using Application.Common;
using Application.DTOs.Reservations.Requests;
using Application.DTOs.Reservations.Responses;

namespace Application.Interfaces.Services.Reservations;

/// <summary>
/// Define la consulta paginada de reservas de una cancha.
/// </summary>
public interface IListCourtReservationsService
{
    Task<PagedResult<ReservationResponse>> ExecuteAsync(
        ListCourtReservationsRequest request,
        CancellationToken cancellationToken = default);
}
