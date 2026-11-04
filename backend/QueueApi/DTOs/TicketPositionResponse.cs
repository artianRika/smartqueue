using QueueApi.Models;

namespace QueueApi.DTOs;

public class TicketPositionResponse
{
    public Guid TicketId { get; set; }

    public TicketStatus Status { get; set; }

    public int? Position { get; set; }
}
