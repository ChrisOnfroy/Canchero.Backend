using Domain.Enums;

namespace Application.DTOs.Courts.Responses;

/// <summary>
/// Contiene una cancha cercana con los datos básicos de su sede y distancia.
/// </summary>
public sealed record NearbyCourtResponse(
    Guid CourtId,
    string CourtName,
    Guid VenueId,
    string VenueName,
    string Address,
    double DistanceMeters,
    double DistanceKilometers,
    SportType SportType,
    SurfaceType SurfaceType,
    bool IsIndoor,
    long PricePerHour,
    Currency Currency,
    long? SlotPrice);
