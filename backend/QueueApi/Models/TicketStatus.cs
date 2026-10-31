namespace QueueApi.Models;

public enum TicketStatus
{
    Waiting = 0,
    Called = 1,
    Serving = 2,
    Completed = 3,
    Cancelled = 4,
    NoShow = 5
}