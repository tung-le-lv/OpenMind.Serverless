namespace Order.Api.Domain.Events;

public record OrderItemAddedEvent(string OrderId, string ProductId, int Quantity) : DomainEventBase
{
    public override string EventType => "OrderItemAdded";
    public override string MessageGroupId => OrderId;
}
