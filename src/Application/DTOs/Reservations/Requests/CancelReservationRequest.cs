namespace Application.DTOs.Reservations.Requests;

/// <summary>
/// Solicita cancelar una reserva, opcionalmente indicando el motivo.
/// </summary>
public sealed record CancelReservationRequest(
    Guid ActorUserId,
    Guid ReservationId,
    string? Reason);
