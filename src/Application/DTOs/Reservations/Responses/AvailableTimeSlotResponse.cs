namespace Application.DTOs.Reservations.Responses;

/// <summary>
/// Representa una franja horaria disponible.
/// </summary>
public sealed record AvailableTimeSlotResponse(DateTimeOffset Start, DateTimeOffset End);
