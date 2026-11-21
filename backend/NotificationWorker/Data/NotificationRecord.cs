namespace NotificationWorker.Data;

public class NotificationRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EventId { get; set; }

    public Guid TicketId { get; set; }

    public Guid QueueId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
