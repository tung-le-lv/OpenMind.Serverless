using Order.Api.Domain.Entities;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Application.Dtos;

public static class OrderMapper
{
    public static OrderDto ToDto(OrderAggregate order) => new(
        Id: order.Id,
        CustomerId: order.CustomerId,
        Items: order.Items.Select(ToItemDto).ToList(),
        TotalAmount: order.TotalAmount.Amount,
        Currency: order.TotalAmount.Currency,
        Status: order.Status,
        ShippingAddress: order.ShippingAddress != null ? ToAddressDto(order.ShippingAddress) : null,
        CreatedAt: order.CreatedAt,
        UpdatedAt: order.UpdatedAt
    );

    private static OrderItemDto ToItemDto(OrderItem item) => new(
        ProductId: item.ProductId,
        ProductName: item.ProductName,
        Quantity: item.Quantity,
        UnitPrice: item.UnitPrice.Amount,
        Subtotal: item.Subtotal.Amount
    );

    private static AddressDto ToAddressDto(Domain.ValueObjects.Address address) => new(
        Street: address.Street,
        City: address.City,
        State: address.State,
        ZipCode: address.ZipCode,
        Country: address.Country
    );
}
