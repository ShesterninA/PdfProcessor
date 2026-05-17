namespace PdfProcessor.API.Infrastructure
{
    public interface IRabbitMqPublisher
    {
        Task PublishAsync(string message);
    }
}
