using Domain.Commons;

namespace Domain.ValueObjects;

/// <summary>
/// Representa un número telefónico normalizado.
/// </summary>
public sealed class PhoneNumber : ValueObject
{
    public PhoneNumber(string value)
    {
        Guard.AgainstEmptyString(value, nameof(value));
        var normalized = value.Trim()
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .Replace("(", string.Empty)
            .Replace(")", string.Empty);
        Guard.AgainstPattern(normalized, @"^\+?[0-9]{7,15}$", nameof(value));
        Value = normalized;
    }

    public string Value { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
}
