using VolcanionPM.Domain.Common;

namespace VolcanionPM.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string Country { get; private set; }
    public string PostalCode { get; private set; }

    private Address(string street, string city, string state, string country, string postalCode)
    {
        Street = street;
        City = city;
        State = state;
        Country = country;
        PostalCode = postalCode;
    }

    public static Address Create(string street, string city, string state, string country, string postalCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty", nameof(country));

        return new Address(street.Trim(), city.Trim(), state.Trim(), country.Trim(), postalCode.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return Country;
        yield return PostalCode;
    }

    public override string ToString() => $"{Street}, {City}, {State}, {Country} {PostalCode}";
}
