namespace QueueApi.Models;

public class CounterAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CounterId { get; set; }

    // References a user in ProfileApi
    public Guid StaffUserId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public DateTime? EndedAt { get; set; }

    public Counter Counter { get; set; } = null!;
}