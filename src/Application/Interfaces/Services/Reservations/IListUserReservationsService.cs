using Application.Common;
using Application.DTOs.Reservations.Requests;
using Application.DTOs.Reservations.Responses;

namespace Application.Interfaces.Services.Reservations;

/// <summary>
/// Define la consulta paginada de reservas de un usuario.
/// </summary>
public interface IListUserReservationsService
{
    Task<PagedResult<ReservationResponse>> ExecuteAsync(
        ListUserReservationsRequest request,
        CancellationToken cancellationToken = default);
}
