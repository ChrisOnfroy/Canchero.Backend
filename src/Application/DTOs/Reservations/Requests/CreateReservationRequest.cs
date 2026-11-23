namespace Application.DTOs.Reservations.Requests;

/// <summary>
/// Solicita una reserva para el actor autenticado.
/// </summary>
public sealed record CreateReservationRequest(
    Guid ActorUserId,
    Guid CourtId,
    DateTimeOffset SlotStart,
    DateTimeOffset SlotEnd);
