using Domain.Commons;
using Domain.Errors;

namespace Domain.ValueObjects;

/// <summary>
/// Representa una distancia en metros.
/// </summary>
public sealed class Distance : ValueObject, IComparable<Distance>
{
    public Distance(double meters)
    {
        if (double.IsNaN(meters) || double.IsInfinity(meters) || meters < 0)
        {
            throw new ValidationException("La distancia no puede ser negativa.");
        }

        Meters = meters;
    }

    public double Meters { get; }
    public double Kilometers => Meters / 1000;

    public int CompareTo(Distance? other)
    {
        Guard.AgainstNull(other, nameof(other));
        return Meters.CompareTo(other!.Meters);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Meters;
    }

    public override string ToString() => $"{Meters} m";

    public static bool operator >(Distance left, Distance right) => left.CompareTo(right) > 0;
    public static bool operator <(Distance left, Distance right) => left.CompareTo(right) < 0;
    public static bool operator >=(Distance left, Distance right) => left.CompareTo(right) >= 0;
    public static bool operator <=(Distance left, Distance right) => left.CompareTo(right) <= 0;
}
