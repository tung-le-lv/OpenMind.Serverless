namespace Order.Api.Domain.ValueObjects;

public sealed class Money(decimal amount, string currency = "USD") : IEquatable<Money>
{
    public decimal Amount { get; } = Math.Round(amount, 2);
    public string Currency { get; } = currency;

    public static Money Zero => new(0, "USD");

    public static Money FromDecimal(decimal amount, string currency = "USD")
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        }

        return new Money(amount, currency);
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        if (result < 0)
        {
            throw new InvalidOperationException("Result cannot be negative.");
        }

        return new Money(result, Currency);
    }

    public Money Multiply(int factor)
    {
        if (factor < 0)
        {
            throw new ArgumentException("Factor cannot be negative.", nameof(factor));
        }

        return new Money(Amount * factor, Currency);
    }

    public bool Equals(Money? other)
    {
        if (other is null)
        {
            return false;
        }
        return Amount == other.Amount && Currency == other.Currency;
    }

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public static bool operator ==(Money? left, Money? right) => left?.Equals(right) ?? right is null;

    public static bool operator !=(Money? left, Money? right) => !(left == right);

    public override string ToString() => $"{Currency} {Amount:F2}";

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException($"Cannot operate on different currencies: {Currency} and {other.Currency}");
        }
    }
}
