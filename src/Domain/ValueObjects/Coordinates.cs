using Domain.Commons;
using Domain.Errors;

namespace Domain.ValueObjects;

/// <summary>
/// Representa una posición geográfica.
/// </summary>
public sealed class Coordinates : ValueObject
{
    private const double EarthRadiusMeters = 6_371_000;

    public Coordinates(double latitude, double longitude)
    {
        if (double.IsNaN(latitude) || double.IsInfinity(latitude) ||
            latitude < -90 || latitude > 90)
        {
            throw new ValidationException("La latitud debe estar entre -90 y 90.");
        }

        if (double.IsNaN(longitude) || double.IsInfinity(longitude) ||
            longitude < -180 || longitude > 180)
        {
            throw new ValidationException("La longitud debe estar entre -180 y 180.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    public double Latitude { get; }
    public double Longitude { get; }

    public Distance DistanceTo(Coordinates other)
    {
        Guard.AgainstNull(other, nameof(other));

        var lat1 = DegreesToRadians(Latitude);
        var lat2 = DegreesToRadians(other.Latitude);
        var latitudeDelta = DegreesToRadians(other.Latitude - Latitude);
        var longitudeDelta = DegreesToRadians(other.Longitude - Longitude);

        var haversine = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2)
            + Math.Cos(lat1) * Math.Cos(lat2)
            * Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2);

        var centralAngle = 2 * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1 - haversine));
        var meters = EarthRadiusMeters * centralAngle;
        return new Distance(meters);
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }
}
