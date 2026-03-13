using FiapX.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace FiapX.Infrastructure.Services;

[ExcludeFromCodeCoverage]
public class RabbitMQService : IMessageQueueService, IDisposable
{
    private readonly ILogger<RabbitMQService> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string QueueName = "video-processing";

    public RabbitMQService(IConfiguration configuration, ILogger<RabbitMQService> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:UserName"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _logger.LogInformation("RabbitMQ conectado e fila {QueueName} declarada", QueueName);
    }

    public Task PublishVideoForProcessingAsync(Guid videoId)
    {
        try
        {
            var message = JsonSerializer.Serialize(new { VideoId = videoId });
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(
                exchange: string.Empty,
                routingKey: QueueName,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Mensagem publicada para processamento do vídeo {VideoId}", videoId);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar mensagem para vídeo {VideoId}", videoId);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}
