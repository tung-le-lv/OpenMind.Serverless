using Order.Api.Domain.Enums;

namespace Order.Api.Domain.Events;

public record OrderStatusChangedEvent(string OrderId, OrderStatus OldStatus, OrderStatus NewStatus) : DomainEventBase
{
    public override string EventType => "OrderStatusChanged";
    public override string MessageGroupId => OrderId;
}
