using Order.Api.Domain.Enums;

namespace Order.Api.Domain.Events;

public class OrderCreatedEvent(string orderId, string customerId) : DomainEventBase
{
    public override string EventType => "OrderCreated";
    public string OrderId { get; } = orderId;
    public string CustomerId { get; } = customerId;
}

public class OrderItemAddedEvent(string orderId, string productId, int quantity) : DomainEventBase
{
    public override string EventType => "OrderItemAdded";
    public string OrderId { get; } = orderId;
    public string ProductId { get; } = productId;
    public int Quantity { get; } = quantity;
}

public class OrderStatusChangedEvent(string orderId, OrderStatus oldStatus, OrderStatus newStatus) : DomainEventBase
{
    public override string EventType => "OrderStatusChanged";
    public string OrderId { get; } = orderId;
    public OrderStatus OldStatus { get; } = oldStatus;
    public OrderStatus NewStatus { get; } = newStatus;
}

public class OrderCancelledEvent(string orderId, OrderStatus previousStatus) : DomainEventBase
{
    public override string EventType => "OrderCancelled";
    public string OrderId { get; } = orderId;
    public OrderStatus PreviousStatus { get; } = previousStatus;
}
