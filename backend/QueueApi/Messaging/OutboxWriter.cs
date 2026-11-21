using System.Text.Json;
using System.Text.Json.Serialization;
using QueueApi.Data;
using QueueApi.Models;

namespace QueueApi.Messaging;

public static class OutboxWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    // Only tracks the event; the caller's SaveChanges writes it in the same transaction as the ticket.
    public static void Add(QueueDbContext context, string type, Ticket ticket)
    {
        var outboxEvent = new OutboxEvent
        {
            Id = Guid.NewGuid(),
            Type = type,
            TicketId = ticket.Id,
            QueueId = ticket.QueueId,
            OccurredAt = DateTime.UtcNow
        };

        var payload = new TicketEvent(
            outboxEvent.Id,
            type,
            ticket.Id,
            ticket.QueueId,
            ticket.CounterId,
            ticket.TicketCode,
            ticket.Status,
            outboxEvent.OccurredAt);

        outboxEvent.Payload = JsonSerializer.Serialize(payload, JsonOptions);

        context.OutboxEvents.Add(outboxEvent);
    }
}
