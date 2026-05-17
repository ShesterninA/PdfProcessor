using Microsoft.EntityFrameworkCore;
using PdfProcessor.API.Infrastructure;
using PdfProcessor.Data;

namespace PdfProcessor.API.Services;

public class OutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRabbitMqPublisher _publisher;
    private readonly ILogger<OutboxWorker> _logger;

    public OutboxWorker(
        IServiceScopeFactory scopeFactory,
        IRabbitMqPublisher publisher,
        ILogger<OutboxWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessages(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessages(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PdfProcessorDbContext>();

        var messages = await context.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(10)
            .ToListAsync(stoppingToken);

        foreach (var message in messages)
        {
            try
            {
                await _publisher.PublishAsync(message.Payload);

                message.ProcessedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "Published outbox message {Id}",
                    message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing outbox message {Id}",
                    message.Id);
            }
        }

        if (messages.Any())
            await context.SaveChangesAsync(stoppingToken);
    }
}