namespace NotificationWorker.Data;

public class ProcessedEvent
{
    public Guid EventId { get; set; }

    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
