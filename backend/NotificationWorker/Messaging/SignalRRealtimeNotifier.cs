using Microsoft.Azure.SignalR.Management;

namespace NotificationWorker.Messaging;

public sealed class SignalRRealtimeNotifier : IRealtimeNotifier, IAsyncDisposable
{
    private const string HubName = "queue";

    private readonly ServiceHubContext _hubContext;
    private readonly ILogger<SignalRRealtimeNotifier> _logger;

    private SignalRRealtimeNotifier(ServiceHubContext hubContext, ILogger<SignalRRealtimeNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public static async Task<SignalRRealtimeNotifier> CreateAsync(
        string connectionString,
        ILogger<SignalRRealtimeNotifier> logger,
        CancellationToken cancellationToken)
    {
        var serviceManager = new ServiceManagerBuilder()
            .WithOptions(options => options.ConnectionString = connectionString)
            .BuildServiceManager();

        var hubContext = await serviceManager.CreateHubContextAsync(HubName, cancellationToken);

        return new SignalRRealtimeNotifier(hubContext, logger);
    }

    public async Task NotifyAsync(
        TicketEventMessage ticketEvent,
        string payload,
        CancellationToken cancellationToken)
    {
        var ticketGroup = $"ticket-{ticketEvent.TicketId}";
        var queueGroup = $"queue-{ticketEvent.QueueId}";

        object[] args = [payload];

        await _hubContext.Clients
            .Group(ticketGroup)
            .SendCoreAsync(ticketEvent.Type, args, cancellationToken);

        await _hubContext.Clients
            .Group(queueGroup)
            .SendCoreAsync(ticketEvent.Type, args, cancellationToken);

        _logger.LogInformation(
            "Pushed {Type} to {TicketGroup} and {QueueGroup}.",
            ticketEvent.Type,
            ticketGroup,
            queueGroup);
    }

    public async ValueTask DisposeAsync()
    {
        await _hubContext.DisposeAsync();
    }
}
