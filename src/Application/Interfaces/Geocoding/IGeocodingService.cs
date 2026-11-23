using Application.DTOs.Geocoding;

namespace Application.Interfaces.Geocoding;

/// <summary>
/// Define la resolución de una ubicación escrita en texto.
/// </summary>
public interface IGeocodingService
{
    Task<GeocodingResult?> GeocodeAsync(
        string query,
        CancellationToken cancellationToken = default);
}
