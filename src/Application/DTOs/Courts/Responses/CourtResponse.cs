using Domain.Enums;

namespace Application.DTOs.Courts.Responses;

/// <summary>
/// Contiene los datos de una cancha.
/// </summary>
public sealed record CourtResponse(
    Guid Id,
    Guid VenueId,
    string Name,
    SportType SportType,
    SurfaceType SurfaceType,
    int Capacity,
    long PricePerHour,
    Currency Currency,
    bool IsIndoor,
    CourtStatus Status);
