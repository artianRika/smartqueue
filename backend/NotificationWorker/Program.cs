using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using NotificationWorker.Data;
using NotificationWorker.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("QueueDb")));

builder.Services.AddScoped<TicketEventProcessor>();

var serviceBusConnectionString = builder.Configuration["ServiceBus:ConnectionString"];

if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
    throw new InvalidOperationException("ServiceBus:ConnectionString is missing.");

builder.Services.AddSingleton(new ServiceBusClient(serviceBusConnectionString));
builder.Services.AddHostedService<TicketEventConsumer>();

var signalRConnectionString = builder.Configuration["SignalR:ConnectionString"];

if (!string.IsNullOrWhiteSpace(signalRConnectionString))
{
    builder.Services.AddSingleton<IRealtimeNotifier>(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<SignalRRealtimeNotifier>>();
        return SignalRRealtimeNotifier
            .CreateAsync(signalRConnectionString, logger, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    });
}
else
{
    builder.Services.AddSingleton<IRealtimeNotifier, NullRealtimeNotifier>();
}

var host = builder.Build();

if (string.IsNullOrWhiteSpace(signalRConnectionString))
{
    host.Services.GetRequiredService<ILoggerFactory>()
        .CreateLogger("NotificationWorker")
        .LogWarning(
            "SignalR:ConnectionString is not set. Events are stored in queue.notifications without realtime push.");
}

host.Run();
