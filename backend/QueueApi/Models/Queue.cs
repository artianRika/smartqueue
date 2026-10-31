namespace QueueApi.Models;

public class Queue
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Organization belongs to ProfileApi
    public Guid OrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    
    public string TicketPrefix { get; set; } = string.Empty;

    public bool IsOpen { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Counter> Counters { get; set; } = new List<Counter>();

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}