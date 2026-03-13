using FiapX.Application.Gateways;
using FiapX.Application.Interfaces.Services;
using FiapX.Application.UseCases.Videos.Command;
using FiapX.Domain.Entities;

namespace FiapX.Application.UseCases.Videos;

public class UploadVideoUseCase
{
    private readonly VideoGateway _gateway;
    private readonly IStorageService _storageService;
    private readonly IMessageQueueService _messageQueueService;

    private UploadVideoUseCase(VideoGateway gateway, IStorageService storageService, IMessageQueueService messageQueueService)
    {
        _gateway = gateway;
        _storageService = storageService;
        _messageQueueService = messageQueueService;
    }

    public static UploadVideoUseCase Create(VideoGateway gateway, IStorageService storageService, IMessageQueueService messageQueueService)
    {
        return new UploadVideoUseCase(gateway, storageService, messageQueueService);
    }

    public async Task<Video> Run(UploadVideoCommand command)
    {
        try
        {
            var videoId = Guid.NewGuid();
            var storagePath = await _storageService.SaveVideoAsync(command.VideoFile, videoId);

            var video = new Video(command.UserId, command.VideoFile.FileName, storagePath);
            video.Id = videoId;

            await _gateway.CreateVideo(video);

            await _messageQueueService.PublishVideoForProcessingAsync(video.Id);

            return video;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao fazer upload do vídeo: {ex.Message}");
        }
    }
}
