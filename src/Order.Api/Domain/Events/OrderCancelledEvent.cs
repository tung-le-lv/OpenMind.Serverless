using Order.Api.Domain.Enums;

namespace Order.Api.Domain.Events;

public record OrderCancelledEvent(string OrderId, OrderStatus PreviousStatus) : DomainEventBase
{
    public override string EventType => "OrderCancelled";
    public override string MessageGroupId => OrderId;
}
