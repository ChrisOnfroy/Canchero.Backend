namespace Application.DTOs.Reservations.Responses;

/// <summary>
/// Contiene las franjas disponibles para la fecha consultada.
/// </summary>
public sealed record GetCourtAvailabilityResponse(
    Guid CourtId,
    DateTimeOffset Day,
    IReadOnlyList<AvailableTimeSlotResponse> AvailableSlots);
