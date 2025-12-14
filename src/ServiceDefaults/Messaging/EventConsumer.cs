namespace ServiceDefaults.Messaging;

public interface IEventConsumer
{
    Task StartAsync(CancellationToken cancellationToken = default);
}

public sealed class NoopEventConsumer : IEventConsumer
{
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Replace with RabbitMQ consumer, include idempotency and dead-letter queues.
        return Task.CompletedTask;
    }
}
