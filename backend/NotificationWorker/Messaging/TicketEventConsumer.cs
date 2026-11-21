using Azure.Messaging.ServiceBus;

namespace NotificationWorker.Messaging;

public class TicketEventConsumer : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TicketEventConsumer> _logger;

    public TicketEventConsumer(
        ServiceBusClient client,
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<TicketEventConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var topic = configuration["ServiceBus:TopicName"] ?? "ticket-events";
        var subscription = configuration["ServiceBus:SubscriptionName"] ?? "notification-worker";

        _processor = client.CreateProcessor(topic, subscription, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += HandleMessageAsync;
        _processor.ProcessErrorAsync += HandleErrorAsync;

        await _processor.StartProcessingAsync(stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
        await _processor.DisposeAsync();
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<TicketEventProcessor>();

            await processor.ProcessAsync(
                args.Message.MessageId,
                args.Message.Body,
                args.CancellationToken);

            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to process Service Bus message {MessageId}.",
                args.Message.MessageId);

            await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
        }
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "Service Bus processor error in {EntityPath} ({ErrorSource}).",
            args.EntityPath,
            args.ErrorSource);

        return Task.CompletedTask;
    }
}
