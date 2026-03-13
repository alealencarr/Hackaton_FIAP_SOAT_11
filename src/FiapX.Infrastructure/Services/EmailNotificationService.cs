using FiapX.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace FiapX.Infrastructure.Services;

[ExcludeFromCodeCoverage]
public class EmailNotificationService : INotificationService, IDisposable
{
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string QueueName = "email-queue";

    public EmailNotificationService(IConfiguration configuration, ILogger<EmailNotificationService> logger)
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

        _logger.LogInformation("EmailNotificationService conectado à fila {Queue}", QueueName);
    }

    public Task SendProcessingCompleteNotificationAsync(string userEmail, string videoName, string downloadUrl)
    {
        var subject = "FIAP X - Seu vídeo foi processado com sucesso!";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>✅ Processamento Concluído!</h2>
                <p>Olá!</p>
                <p>O processamento do seu vídeo <strong>{videoName}</strong> foi concluído com sucesso.</p>
                <p>Acesse o sistema para fazer o download das imagens extraídas.</p>
                <br/>
                <p>Atenciosamente,<br/>Equipe FIAP X</p>
            </body>
            </html>";

        return PublishEmailAsync(userEmail, subject, body);
    }

    public Task SendProcessingFailedNotificationAsync(string userEmail, string videoName, string errorMessage)
    {
        var subject = "FIAP X - Falha no processamento do vídeo";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>❌ Falha no Processamento</h2>
                <p>Olá!</p>
                <p>Houve um erro ao processar o vídeo <strong>{videoName}</strong>.</p>
                <p><strong>Erro:</strong> {errorMessage}</p>
                <p>Por favor, tente novamente.</p>
                <br/>
                <p>Atenciosamente,<br/>Equipe FIAP X</p>
            </body>
            </html>";

        return PublishEmailAsync(userEmail, subject, body);
    }

    private Task PublishEmailAsync(string to, string subject, string body)
    {
        try
        {
            var emailMessage = new { To = to, Subject = subject, Body = body, IsHtml = true };
            var message = JsonSerializer.Serialize(emailMessage);
            var messageBody = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(
                exchange: string.Empty,
                routingKey: QueueName,
                basicProperties: properties,
                body: messageBody);

            _logger.LogInformation("📧 E-mail enfileirado para {To}: {Subject}", to, subject);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enfileirar e-mail para {To}", to);
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
