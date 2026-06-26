using Order.Api.Domain.ValueObjects;

namespace Order.Api.Domain.Entities;

public record OrderItem(string ProductId, string ProductName, int Quantity, Money UnitPrice)
{
    public Money Subtotal => Money.FromDecimal(Quantity * UnitPrice.Amount);

    public static OrderItem Create(string productId, string productName, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            throw new DomainException("Product ID is required.");
        }

        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new DomainException("Product name is required.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }

        if (unitPrice < 0)
        {
            throw new DomainException("Unit price cannot be negative.");
        }

        return new OrderItem(productId, productName, quantity, Money.FromDecimal(unitPrice));
    }

    public static OrderItem Reconstitute(string productId, string productName, int quantity, decimal unitPrice)
    {
        return new OrderItem(productId, productName, quantity, Money.FromDecimal(unitPrice));
    }

    public OrderItem IncreaseQuantity(int amount)
    {
        if (amount <= 0)
        {
            throw new DomainException("Amount must be greater than zero.");
        }

        return this with { Quantity = Quantity + amount };
    }

    public OrderItem DecreaseQuantity(int amount)
    {
        if (amount <= 0)
        {
            throw new DomainException("Amount must be greater than zero.");
        }

        if (Quantity - amount < 1)
        {
            throw new DomainException("Quantity cannot be less than 1.");
        }

        return this with { Quantity = Quantity - amount };
    }

    public OrderItem UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }

        return this with { Quantity = newQuantity };
    }
}
