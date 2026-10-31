namespace QueueApi.Models;

public class Ticket
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid QueueId { get; set; }

    // References a user in ProfileApi
    public Guid UserId { get; set; }

    // Assigned when staff calls the ticket
    public Guid? CounterId { get; set; }

    // Sequential prefix and number combinaiton within the queue
    public int Number { get; set; }

    public string TicketCode { get; set; } = string.Empty;

    public DateOnly TicketDate { get; set; }

    public TicketStatus Status { get; set; } = TicketStatus.Waiting;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CalledAt { get; set; }

    public DateTime? ServiceStartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public Queue Queue { get; set; } = null!;

    public Counter? Counter { get; set; }
}