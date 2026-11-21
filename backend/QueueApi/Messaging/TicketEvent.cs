using QueueApi.Models;

namespace QueueApi.Messaging;

public static class TicketEventTypes
{
    public const string TicketCreated = "TicketCreated";
    public const string TicketCalled = "TicketCalled";
    public const string TicketServiceStarted = "TicketServiceStarted";
    public const string TicketCompleted = "TicketCompleted";
    public const string TicketCancelled = "TicketCancelled";
    public const string TicketNoShow = "TicketNoShow";
}

public record TicketEvent(
    Guid EventId,
    string Type,
    Guid TicketId,
    Guid QueueId,
    Guid? CounterId,
    string TicketCode,
    TicketStatus Status,
    DateTime OccurredAt);
