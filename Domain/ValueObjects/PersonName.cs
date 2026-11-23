using Domain.Commons;

namespace Domain.ValueObjects;

/// <summary>
/// Representa el nombre de una persona.
/// </summary>
public sealed class PersonName : ValueObject
{
    public PersonName(string firstName, string lastName)
    {
        Guard.AgainstEmptyString(firstName, nameof(firstName));
        Guard.AgainstEmptyString(lastName, nameof(lastName));
        Guard.AgainstMinLength(firstName.Trim(), 2, nameof(firstName));
        Guard.AgainstMaxLength(firstName.Trim(), 50, nameof(firstName));
        Guard.AgainstMinLength(lastName.Trim(), 2, nameof(lastName));
        Guard.AgainstMaxLength(lastName.Trim(), 50, nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public string FirstName { get; }
    public string LastName { get; }
    public string FullName => $"{FirstName} {LastName}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }

    public override string ToString() => FullName;
}
