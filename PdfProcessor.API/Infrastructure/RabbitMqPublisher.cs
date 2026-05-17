using Microsoft.Extensions.Options;
using PdfProcessor.API.Settings;
using RabbitMQ.Client;
using System.Text;

namespace PdfProcessor.API.Infrastructure;

public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _queueName;

    public RabbitMqPublisher(IOptions<RabbitMqSettings> settings)
    {
        var s = settings.Value;
        var factory = new ConnectionFactory
        {
            HostName = s.Host,
            Port = s.Port,
            UserName = s.Username,
            Password = s.Password
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        _queueName = s.QueueName;

        _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false).GetAwaiter().GetResult();
    }

    public async Task PublishAsync(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        await _channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _queueName,
            body: body);
    }

    public void Dispose()
    {
        _channel?.CloseAsync().GetAwaiter().GetResult();
        _connection?.CloseAsync().GetAwaiter().GetResult();
    }
}