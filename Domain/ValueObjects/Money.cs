using Domain.Commons;
using Domain.Enums;
using Domain.Errors;

namespace Domain.ValueObjects;

/// <summary>
/// Representa un importe en la unidad mínima de una moneda.
/// </summary>
public sealed class Money : ValueObject, IComparable<Money>
{
    public Money(long amount, Currency currency)
    {
        Guard.AgainstNegative(amount, nameof(amount));
        Amount = amount;
        Currency = currency;
    }

    public long Amount { get; }
    public Currency Currency { get; }
    public bool IsZero => Amount == 0;

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal multiplier)
    {
        var result = decimal.Round(Amount * multiplier, 0, MidpointRounding.AwayFromZero);
        if (result < 0)
        {
            throw new ValidationException("El monto multiplicado no puede ser negativo.");
        }

        if (result > long.MaxValue)
        {
            throw new ValidationException("El monto excede el rango permitido.");
        }

        return new Money((long)result, Currency);
    }

    public bool IsGreaterThan(Money other)
    {
        EnsureSameCurrency(other);
        return Amount > other.Amount;
    }

    private void EnsureSameCurrency(Money other)
    {
        Guard.AgainstNull(other, nameof(other));
        if (Currency != other.Currency)
        {
            throw new BusinessRuleException("No se pueden operar montos con monedas diferentes.");
        }
    }

    public int CompareTo(Money? other)
    {
        Guard.AgainstNull(other, nameof(other));
        EnsureSameCurrency(other!);
        return Amount.CompareTo(other!.Amount);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount} {Currency}";

    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;
    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;
    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;
    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;
}
