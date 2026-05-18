using Microsoft.Extensions.Options;
using PdfProcessor.Data;
using PdfProcessor.Data.Repositories;
using PdfProcessor.Shared.Enums;
using PdfProcessor.Worker.Models;
using PdfProcessor.Worker.Services;
using PdfProcessor.Worker.Settings;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PdfProcessor.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly RabbitMqSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly StorageSettings _storageSettings;
    private IConnection? _connection;
    private IChannel? _channel;

    public Worker(
        ILogger<Worker> logger,
        IOptions<RabbitMqSettings> settings,
        IServiceScopeFactory scopeFactory,
        IOptions<StorageSettings> storageSettings)
    {
        _logger = logger;
        _settings = settings.Value;
        _scopeFactory = scopeFactory;
        _storageSettings = storageSettings.Value;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(10, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            onRetry: (ex, delay) =>
                _logger.LogWarning(ex, "RabbitMQ недоступен, повтор через {Delay} сек", delay.TotalSeconds));

        await retryPolicy.ExecuteAsync(async () =>
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password
            };
            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            await _channel.QueueDeclareAsync(
                queue: _settings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);
        });

        _logger.LogInformation("RabbitMQ connection established");
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel is null)
            throw new InvalidOperationException("RabbitMQ channel is not initialized");

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<DocumentUploadedMessage>(json);

                if (message is null)
                {
                    _logger.LogWarning("Received invalid message");
                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                    return;
                }

                _logger.LogInformation("Received document uploaded event: {DocumentId}", message.Id);
                await ProcessDocumentAsync(message.Id);
                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing rabbitmq message");
                await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        await _channel.BasicConsumeAsync(queue: _settings.QueueName, autoAck: false, consumer: consumer);
        _logger.LogInformation("RabbitMQ consumer started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessDocumentAsync(Guid documentId)
    {
        using var scope = _scopeFactory.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IPdfDocumentRepository>();
        var context = scope.ServiceProvider.GetRequiredService<PdfProcessorDbContext>();
        var extractor = scope.ServiceProvider.GetRequiredService<IPdfTextExtractor>();
        var document = await repository.GetTrackedByIdAsync(documentId);

        if (document is null)
        {
            _logger.LogWarning("Document {DocumentId} not found", documentId);
            return;
        }

        try
        {
            document.Status = DocumentStatus.Processing;
            await context.SaveChangesAsync();

            var filePath = Path.Combine(_storageSettings.UploadPdfPath, $"{documentId}.pdf");
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Pdf file not found: {filePath}");
            }

            var text = extractor.ExtractText(filePath);
            document.Content = text;
            document.Status = DocumentStatus.Completed;
            await context.SaveChangesAsync();

            _logger.LogInformation("Document {DocumentId} processed successfully", documentId);
        }
        catch (Exception ex)
        {
            document.Status = DocumentStatus.Failed;
            await context.SaveChangesAsync();
            _logger.LogError(ex, "Error processing document {DocumentId}", documentId);
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
            await _channel.DisposeAsync();

        if (_connection is not null)
            await _connection.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }
}