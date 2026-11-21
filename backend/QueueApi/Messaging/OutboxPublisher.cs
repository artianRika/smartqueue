using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using QueueApi.Data;
using QueueApi.Models;

namespace QueueApi.Messaging;

public class OutboxPublisher : BackgroundService
{
    private const int MaxErrorLength = 2000;
    private static readonly TimeSpan FailureBackoff = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<OutboxPublisher> _logger;
    private readonly int _batchSize;
    private readonly TimeSpan _pollInterval;

    public OutboxPublisher(
        IServiceScopeFactory scopeFactory,
        ServiceBusClient client,
        IConfiguration configuration,
        ILogger<OutboxPublisher> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _sender = client.CreateSender(configuration["ServiceBus:TopicName"] ?? "ticket-events");
        _batchSize = configuration.GetValue("Outbox:BatchSize", 50);
        _pollInterval = TimeSpan.FromMilliseconds(configuration.GetValue("Outbox:PollIntervalMs", 1000));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var published = await PublishBatchAsync(stoppingToken);

                // A full batch means more rows are probably waiting, so poll again immediately
                if (published < _batchSize)
                    await Task.Delay(_pollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Publishing outbox events failed.");
                await Task.Delay(FailureBackoff, stoppingToken);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        await _sender.DisposeAsync();
    }

    private async Task<int> PublishBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<QueueDbContext>();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        // SKIP LOCKED lets several QueueApi replicas publish without sending the same row twice
        var pending = await context.OutboxEvents
            .FromSqlRaw(
                """
                SELECT *
                FROM queue.outbox_events
                WHERE "PublishedAt" IS NULL
                ORDER BY "OccurredAt"
                LIMIT {0}
                FOR UPDATE SKIP LOCKED
                """,
                _batchSize)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return 0;
        }

        try
        {
            await _sender.SendMessagesAsync(pending.Select(ToMessage), cancellationToken);

            var publishedAt = DateTime.UtcNow;

            foreach (var outboxEvent in pending)
                outboxEvent.PublishedAt = publishedAt;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            foreach (var outboxEvent in pending)
            {
                outboxEvent.Attempts++;
                outboxEvent.LastError = Truncate(exception.Message);
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            throw;
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("Published {Count} outbox events.", pending.Count);

        return pending.Count;
    }

    private static ServiceBusMessage ToMessage(OutboxEvent outboxEvent)
    {
        // MessageId lets Service Bus duplicate detection drop a re-send after a crash
        var message = new ServiceBusMessage(BinaryData.FromString(outboxEvent.Payload))
        {
            MessageId = outboxEvent.Id.ToString(),
            Subject = outboxEvent.Type,
            ContentType = "application/json"
        };

        message.ApplicationProperties["ticketId"] = outboxEvent.TicketId.ToString();
        message.ApplicationProperties["queueId"] = outboxEvent.QueueId.ToString();

        return message;
    }

    private static string Truncate(string value)
    {
        return value.Length <= MaxErrorLength ? value : value[..MaxErrorLength];
    }
}
