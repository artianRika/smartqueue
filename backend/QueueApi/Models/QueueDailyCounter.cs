namespace QueueApi.Models;

public class QueueDailyCounter
{
    public Guid QueueId { get; set; }

    public DateOnly TicketDate { get; set; }

    public int LastNumber { get; set; }

    public Queue Queue { get; set; } = null!;
}