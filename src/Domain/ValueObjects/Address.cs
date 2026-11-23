using Domain.Commons;

namespace Domain.ValueObjects;

/// <summary>
/// Representa una dirección postal.
/// </summary>
public sealed class Address : ValueObject
{
    public Address(string street, string city, string department, string country)
    {
        Guard.AgainstEmptyString(street, nameof(street));
        Guard.AgainstEmptyString(city, nameof(city));
        Guard.AgainstEmptyString(department, nameof(department));
        Guard.AgainstEmptyString(country, nameof(country));

        Street = street.Trim();
        City = city.Trim();
        Department = department.Trim();
        Country = country.Trim();
    }

    public string Street { get; }
    public string City { get; }
    public string Department { get; }
    public string Country { get; }
    public string FullAddress => $"{Street}, {City}, {Department}, {Country}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return Department;
        yield return Country;
    }

    public override string ToString() => FullAddress;
}
