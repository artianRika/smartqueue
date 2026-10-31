namespace QueueApi.Models;

public class Counter
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid QueueId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsOpen { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Queue Queue { get; set; } = null!;

    public ICollection<CounterAssignment> Assignments { get; set; }
        = new List<CounterAssignment>();

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}