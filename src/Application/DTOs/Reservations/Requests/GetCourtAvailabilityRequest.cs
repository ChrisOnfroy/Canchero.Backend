namespace Application.DTOs.Reservations.Requests;

/// <summary>
/// Solicita disponibilidad usando la fecha local de Day y su offset como zona horaria de referencia.
/// </summary>
public sealed record GetCourtAvailabilityRequest(
    Guid ActorUserId,
    Guid CourtId,
    DateTimeOffset Day);
