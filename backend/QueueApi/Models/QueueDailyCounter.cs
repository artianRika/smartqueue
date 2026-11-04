using System.Text.Json.Serialization;

namespace QueueApi.Models;

public class QueueDailyCounter
{
    public Guid QueueId { get; set; }

    public DateOnly TicketDate { get; set; }

    public int LastNumber { get; set; }

    [JsonIgnore]
    public Queue Queue { get; set; } = null!;
}