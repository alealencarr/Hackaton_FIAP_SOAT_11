namespace FiapX.Application.Interfaces.Services;

public interface IMessageQueueService
{
    Task PublishVideoForProcessingAsync(Guid videoId);
}
