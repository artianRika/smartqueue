using Microsoft.Azure.SignalR.Management;

namespace QueueApi.Messaging;

public sealed class SignalRNegotiateService : IAsyncDisposable
{
    public const string HubName = "queue";

    private readonly ServiceManager _serviceManager;
    private readonly SemaphoreSlim _hubLock = new(1, 1);
    private ServiceHubContext? _hubContext;

    public SignalRNegotiateService(string connectionString)
    {
        _serviceManager = new ServiceManagerBuilder()
            .WithOptions(options => options.ConnectionString = connectionString)
            .BuildServiceManager();
    }

    public async Task<SignalRNegotiateResponse> NegotiateAsync(
        string userId,
        Guid? ticketId,
        Guid? queueId,
        CancellationToken cancellationToken)
    {
        var hub = await GetHubContextAsync(cancellationToken);

        if (ticketId is Guid ticket)
            await hub.UserGroups.AddToGroupAsync(userId, $"ticket-{ticket}", cancellationToken);

        if (queueId is Guid queue)
            await hub.UserGroups.AddToGroupAsync(userId, $"queue-{queue}", cancellationToken);

        var negotiation = await hub.NegotiateAsync(
            new NegotiationOptions { UserId = userId },
            cancellationToken);

        return new SignalRNegotiateResponse(negotiation.Url!, negotiation.AccessToken!);
    }

    private async Task<ServiceHubContext> GetHubContextAsync(CancellationToken cancellationToken)
    {
        if (_hubContext is not null)
            return _hubContext;

        await _hubLock.WaitAsync(cancellationToken);

        try
        {
            _hubContext ??= await _serviceManager.CreateHubContextAsync(HubName, cancellationToken);
            return _hubContext;
        }
        finally
        {
            _hubLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubContext is not null)
            await _hubContext.DisposeAsync();

        _hubLock.Dispose();
    }
}

public record SignalRNegotiateResponse(string Url, string AccessToken);
