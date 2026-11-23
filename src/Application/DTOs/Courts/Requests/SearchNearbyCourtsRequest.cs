using Domain.Enums;

namespace Application.DTOs.Courts.Requests;

/// <summary>
/// Solicita canchas cercanas a una ubicación y, opcionalmente, disponibles en un horario.
/// </summary>
public sealed record SearchNearbyCourtsRequest(
    Guid ActorUserId,
    double? Latitude = null,
    double? Longitude = null,
    double RadiusKm = 10,
    SportType? SportType = null,
    SurfaceType? SurfaceType = null,
    bool? IsIndoor = null,
    long? MaxPricePerHour = null,
    Currency PriceCurrency = Currency.COP,
    DateTimeOffset? SlotStart = null,
    DateTimeOffset? SlotEnd = null,
    int Page = 1,
    int PageSize = 10,
    string? LocationText = null);
