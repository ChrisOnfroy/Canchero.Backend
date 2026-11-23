namespace Application.DTOs.Reservations.Requests;

/// <summary>
/// Solicita confirmar una reserva.
/// </summary>
public sealed record ConfirmReservationRequest(Guid ActorUserId, Guid ReservationId);
