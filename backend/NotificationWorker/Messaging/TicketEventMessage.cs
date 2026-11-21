namespace NotificationWorker.Messaging;

public record TicketEventMessage(
    Guid EventId,
    string Type,
    Guid TicketId,
    Guid QueueId,
    Guid? CounterId,
    string TicketCode,
    string Status,
    DateTime OccurredAt);
