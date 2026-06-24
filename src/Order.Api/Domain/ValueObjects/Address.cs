namespace Order.Api.Domain.ValueObjects;

public sealed class Address(string street, string city, string state, string zipCode, string country) : IEquatable<Address>
{
    public string Street { get; } = street;
    public string City { get; } = city;
    public string State { get; } = state;
    public string ZipCode { get; } = zipCode;
    public string Country { get; } = country;

    public static Address Create(string street, string city, string state, string zipCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
        {
            throw new ArgumentException("Street is required.", nameof(street));
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("City is required.", nameof(city));
        }

        if (string.IsNullOrWhiteSpace(state))
        {
            throw new ArgumentException("State is required.", nameof(state));
        }

        if (string.IsNullOrWhiteSpace(zipCode))
        {
            throw new ArgumentException("Zip code is required.", nameof(zipCode));
        }

        if (string.IsNullOrWhiteSpace(country))
        {
            throw new ArgumentException("Country is required.", nameof(country));
        }

        return new Address(street.Trim(), city.Trim(), state.Trim(), zipCode.Trim(), country.Trim());
    }

    public bool Equals(Address? other)
    {
        if (other is null)
        {
            return false;
        }
        return Street == other.Street &&
               City == other.City &&
               State == other.State &&
               ZipCode == other.ZipCode &&
               Country == other.Country;
    }

    public override bool Equals(object? obj) => Equals(obj as Address);

    public override int GetHashCode() => HashCode.Combine(Street, City, State, ZipCode, Country);

    public static bool operator ==(Address? left, Address? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(Address? left, Address? right) => !(left == right);

    public override string ToString() => $"{Street}, {City}, {State} {ZipCode}, {Country}";
}
