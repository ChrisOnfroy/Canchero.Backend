using Domain.Commons;
using Domain.Errors;

namespace Domain.ValueObjects;

/// <summary>
/// Representa la capacidad de jugadores de una cancha.
/// </summary>
public sealed class Capacity : ValueObject
{
    public Capacity(int players)
    {
        if (players is < 1 or > 100)
        {
            throw new ValidationException("La capacidad debe estar entre 1 y 100 jugadores.");
        }

        Players = players;
    }

    public int Players { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Players;
    }

    public override string ToString() => Players.ToString();
}
