using Application.Common;

namespace Application.DTOs.Courts.Responses;

/// <summary>
/// Contiene el origen de la búsqueda y la página de canchas cercanas.
/// </summary>
public sealed record SearchNearbyCourtsResponse(
    double OriginLatitude,
    double OriginLongitude,
    string? FormattedAddress,
    PagedResult<NearbyCourtResponse> Results);
