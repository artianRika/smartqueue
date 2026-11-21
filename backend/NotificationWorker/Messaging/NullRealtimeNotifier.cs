namespace NotificationWorker.Messaging;

public class NullRealtimeNotifier(ILogger<NullRealtimeNotifier> logger) : IRealtimeNotifier
{
    public Task NotifyAsync(
        TicketEventMessage ticketEvent,
        string payload,
        CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "SignalR is not configured; skipped push for {Type} ticket {TicketId}.",
            ticketEvent.Type,
            ticketEvent.TicketId);

        return Task.CompletedTask;
    }
}
