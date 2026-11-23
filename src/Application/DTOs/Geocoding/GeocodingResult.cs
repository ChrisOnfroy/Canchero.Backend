using Domain.ValueObjects;

namespace Application.DTOs.Geocoding;

/// <summary>
/// Contiene las coordenadas y la dirección normalizada de una ubicación.
/// </summary>
public sealed record GeocodingResult(Coordinates Coordinates, string FormattedAddress);
