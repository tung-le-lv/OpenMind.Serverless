using Order.Api.Domain.Events;
using Order.Api.Domain.Interfaces;

namespace Order.Api.Infrastructure.EventBus;

public class InMemoryEventBus : IEventBus
{
    private readonly List<IDomainEvent> _events = [];

    public IReadOnlyList<IDomainEvent> PublishedEvents => _events.AsReadOnly();

    public Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : IDomainEvent
    {
        _events.Add(domainEvent);
        Console.WriteLine($"[EventBus] Published event: {domainEvent.EventType} (ID: {domainEvent.EventId})");
        return Task.CompletedTask;
    }

    public void Clear()
    {
        _events.Clear();
    }
}
