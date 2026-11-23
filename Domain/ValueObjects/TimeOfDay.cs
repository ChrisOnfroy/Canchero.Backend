using Domain.Commons;
using Domain.Errors;

namespace Domain.ValueObjects;

/// <summary>
/// Representa una hora del día.
/// </summary>
public sealed class TimeOfDay : ValueObject
{
    public TimeOfDay(int hour, int minute)
    {
        if (hour is < 0 or > 23)
        {
            throw new ValidationException("La hora debe estar entre 0 y 23.");
        }

        if (minute is < 0 or > 59)
        {
            throw new ValidationException("Los minutos deben estar entre 0 y 59.");
        }

        Hour = hour;
        Minute = minute;
    }

    public int Hour { get; }
    public int Minute { get; }

    public int ToMinutes() => Hour * 60 + Minute;
    public bool IsBefore(TimeOfDay other) => ToMinutes() < other.ToMinutes();
    public bool IsAfter(TimeOfDay other) => ToMinutes() > other.ToMinutes();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Hour;
        yield return Minute;
    }

    public override string ToString() => $"{Hour:D2}:{Minute:D2}";
}
