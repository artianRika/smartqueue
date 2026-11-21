namespace NotificationWorker.Messaging;

public interface IRealtimeNotifier
{
    Task NotifyAsync(TicketEventMessage ticketEvent, string payload, CancellationToken cancellationToken);
}
