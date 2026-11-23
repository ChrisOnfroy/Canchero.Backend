namespace Application.DTOs.Reservations.Requests;

/// <summary>
/// Solicita una página de reservas de una cancha.
/// </summary>
public sealed record ListCourtReservationsRequest(
    Guid ActorUserId,
    Guid CourtId,
    int Page = 1,
    int PageSize = 10);
