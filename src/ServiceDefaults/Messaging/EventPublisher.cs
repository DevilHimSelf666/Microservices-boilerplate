namespace ServiceDefaults.Messaging;

public interface IEventPublisher
{
    Task PublishAsync(BaseEvent @event, CancellationToken cancellationToken = default);
}

public sealed class NoopEventPublisher : IEventPublisher
{
    public Task PublishAsync(BaseEvent @event, CancellationToken cancellationToken = default)
    {
        // TODO: Replace with RabbitMQ publisher implementation and transactional outbox integration.
        return Task.CompletedTask;
    }
}
