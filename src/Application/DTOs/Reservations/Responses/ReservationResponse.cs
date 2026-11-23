using Domain.Enums;

namespace Application.DTOs.Reservations.Responses;

/// <summary>
/// Contiene los datos de una reserva.
/// </summary>
public sealed record ReservationResponse(
    Guid Id,
    Guid UserId,
    Guid CourtId,
    DateTimeOffset Start,
    DateTimeOffset End,
    ReservationStatus Status,
    long TotalPrice,
    Currency Currency,
    string? CancellationReason);
