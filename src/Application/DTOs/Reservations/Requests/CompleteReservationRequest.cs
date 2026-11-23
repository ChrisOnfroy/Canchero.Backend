namespace Application.DTOs.Reservations.Requests;

/// <summary>
/// Solicita completar una reserva finalizada.
/// </summary>
public sealed record CompleteReservationRequest(Guid ActorUserId, Guid ReservationId);
