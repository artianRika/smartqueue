namespace QueueApi.Models;

public class OutboxEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Type { get; set; } = string.Empty;

    public Guid TicketId { get; set; }

    public Guid QueueId { get; set; }

    // Serialized event body sent as the Service Bus message
    public string Payload { get; set; } = string.Empty;

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    // Null until the publisher has sent the event to Service Bus
    public DateTime? PublishedAt { get; set; }

    public int Attempts { get; set; }

    public string? LastError { get; set; }
}
