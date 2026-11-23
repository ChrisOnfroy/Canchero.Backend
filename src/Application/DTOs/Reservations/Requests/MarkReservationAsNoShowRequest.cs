namespace Application.DTOs.Reservations.Requests;

/// <summary>
/// Solicita marcar una reserva como inasistencia.
/// </summary>
public sealed record MarkReservationAsNoShowRequest(Guid ActorUserId, Guid ReservationId);
