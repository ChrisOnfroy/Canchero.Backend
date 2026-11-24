using Domain.ValueObjects;

namespace Application.Common;

/// <summary>
/// Rectángulo de latitud y longitud que contiene un círculo de búsqueda. Sirve para descartar
/// sedes lejanas en la base de datos antes de calcular la distancia exacta.
/// </summary>
/// <param name="MinLatitude">Latitud mínima.</param>
/// <param name="MaxLatitude">Latitud máxima.</param>
/// <param name="MinLongitude">Longitud mínima, o <c>null</c> si no se puede acotar (polos o antimeridiano).</param>
/// <param name="MaxLongitude">Longitud máxima, o <c>null</c> si no se puede acotar.</param>
public sealed record GeoBounds(
    double MinLatitude,
    double MaxLatitude,
    double? MinLongitude,
    double? MaxLongitude)
{
    private const double KilometersPerDegree = 111.195;
    // Margen para que el rectángulo siempre contenga el círculo exacto calculado con Haversine.
    private const double SafetyFactor = 1.02;

    public static GeoBounds Around(Coordinates center, double radiusKm)
    {
        var latitudeDelta = radiusKm / KilometersPerDegree * SafetyFactor;
        var minLatitude = center.Latitude - latitudeDelta;
        var maxLatitude = center.Latitude + latitudeDelta;

        var cosine = Math.Cos(center.Latitude * Math.PI / 180);
        if (cosine < 0.01 || minLatitude < -90 || maxLatitude > 90)
        {
            // Cerca de los polos la longitud no sirve para acotar.
            return new GeoBounds(minLatitude, maxLatitude, null, null);
        }

        var longitudeDelta = radiusKm / (KilometersPerDegree * cosine) * SafetyFactor;
        var minLongitude = center.Longitude - longitudeDelta;
        var maxLongitude = center.Longitude + longitudeDelta;
        if (minLongitude < -180 || maxLongitude > 180)
        {
            // El círculo cruza el antimeridiano: se filtra solo por latitud.
            return new GeoBounds(minLatitude, maxLatitude, null, null);
        }

        return new GeoBounds(minLatitude, maxLatitude, minLongitude, maxLongitude);
    }
}
