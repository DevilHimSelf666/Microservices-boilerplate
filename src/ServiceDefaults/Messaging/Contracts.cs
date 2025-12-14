namespace ServiceDefaults.Messaging;

public record BaseEvent(string EventType, int Version, DateTime OccurredOnUtc);

public record ExampleEvent(string Payload) : BaseEvent("example", 1, DateTime.UtcNow);
