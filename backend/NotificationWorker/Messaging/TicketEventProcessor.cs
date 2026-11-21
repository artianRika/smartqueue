using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NotificationWorker.Data;
using Npgsql;

namespace NotificationWorker.Messaging;

public class TicketEventProcessor(
    NotificationDbContext db,
    IRealtimeNotifier notifier,
    ILogger<TicketEventProcessor> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task ProcessAsync(
        string messageId,
        BinaryData body,
        CancellationToken cancellationToken)
    {
        var payload = body.ToString();
        var ticketEvent = JsonSerializer.Deserialize<TicketEventMessage>(payload, JsonOptions);

        if (ticketEvent is null)
            throw new InvalidOperationException("Ticket event payload could not be deserialized.");

        var eventId = Guid.TryParse(messageId, out var fromMessageId)
            ? fromMessageId
            : ticketEvent.EventId;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            db.ProcessedEvents.Add(new ProcessedEvent
            {
                EventId = eventId,
                ProcessedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogInformation("Skipping duplicate event {EventId}.", eventId);
            return;
        }

        db.Notifications.Add(new NotificationRecord
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            TicketId = ticketEvent.TicketId,
            QueueId = ticketEvent.QueueId,
            Type = ticketEvent.Type,
            Payload = payload,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await notifier.NotifyAsync(ticketEvent, payload, cancellationToken);

        logger.LogInformation(
            "Processed {Type} for ticket {TicketId} (event {EventId}).",
            ticketEvent.Type,
            ticketEvent.TicketId,
            eventId);
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        Exception? current = exception;

        while (current is not null)
        {
            if (current is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
                return true;

            current = current.InnerException;
        }

        return false;
    }
}
