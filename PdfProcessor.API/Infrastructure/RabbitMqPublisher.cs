using Microsoft.Extensions.Options;
using PdfProcessor.API.Settings;
using Polly;
using RabbitMQ.Client;
using System.Text;

namespace PdfProcessor.API.Infrastructure;

public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly Lazy<Task<IConnection>> _lazyConnection;
    private bool _disposed;

    public RabbitMqPublisher(IOptions<RabbitMqSettings> settings)
    {
        _settings = settings.Value;

        _lazyConnection = new Lazy<Task<IConnection>>(async () =>
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(10, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

            return await retryPolicy.ExecuteAsync(async () =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = _settings.Host,
                    Port = _settings.Port,
                    UserName = _settings.Username,
                    Password = _settings.Password,
                    AutomaticRecoveryEnabled = true
                };

                return await factory.CreateConnectionAsync();
            });
        });
    }

    public async Task PublishAsync(string message)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RabbitMqPublisher));

        var connection = await _lazyConnection.Value;

        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: _settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _settings.QueueName,
            body: body);
    }

    public async void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_lazyConnection.IsValueCreated)
        {
            var connection = await _lazyConnection.Value;
            await connection.CloseAsync();
        }
    }
}