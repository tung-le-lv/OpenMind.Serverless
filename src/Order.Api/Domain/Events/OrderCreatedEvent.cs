namespace Order.Api.Domain.Events;

public record OrderCreatedEvent(string OrderId, string CustomerId) : DomainEventBase
{
    public override string EventType => "OrderCreated";
}
