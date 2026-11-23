using Domain.Commons;
using Domain.Errors;

namespace Domain.ValueObjects;

/// <summary>
/// Representa un intervalo de tiempo.
/// </summary>
public sealed class TimeSlot : ValueObject
{
    public TimeSlot(DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
        {
            throw new ValidationException("La hora de finalización debe ser posterior a la de inicio.");
        }

        Start = start;
        End = end;
    }

    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }
    public double DurationInMinutes => (End - Start).TotalMinutes;

    public bool OverlapsWith(TimeSlot other)
    {
        Guard.AgainstNull(other, nameof(other));
        return Start < other.End && other.Start < End;
    }

    public bool IsSameDay()
    {
        return Start.Date == End.Date;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }

    public override string ToString() => $"{Start:O} - {End:O}";
}
