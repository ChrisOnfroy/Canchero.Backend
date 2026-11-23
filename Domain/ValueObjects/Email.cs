using System.Text.RegularExpressions;
using Domain.Commons;

namespace Domain.ValueObjects;

/// <summary>
/// Representa una dirección de correo electrónico válida.
/// </summary>
public sealed class Email : ValueObject
{
    private static readonly Regex FormatRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public Email(string value)
    {
        Guard.AgainstEmptyString(value, nameof(value));
        var normalized = value.Trim().ToLowerInvariant();
        Guard.AgainstMaxLength(normalized, 254, nameof(value));
        Guard.AgainstPattern(normalized, FormatRegex.ToString(), nameof(value));
        Value = normalized;
    }

    public string Value { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
