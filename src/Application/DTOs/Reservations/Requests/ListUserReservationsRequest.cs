using Domain.Enums;

namespace Application.DTOs.Reservations.Requests;

/// <summary>
/// Solicita una página de reservas de un usuario.
/// </summary>
public sealed record ListUserReservationsRequest(
    Guid ActorUserId,
    Guid UserId,
    int Page = 1,
    int PageSize = 10,
    ReservationStatus? Status = null);
