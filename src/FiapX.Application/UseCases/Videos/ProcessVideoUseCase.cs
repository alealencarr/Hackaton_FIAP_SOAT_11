using FiapX.Application.Gateways;
using FiapX.Application.Interfaces.Services;
using FiapX.Application.UseCases.Videos.Command;
using FiapX.Domain.Entities;

namespace FiapX.Application.UseCases.Videos;

public class ProcessVideoUseCase
{
    private readonly VideoGateway _videoGateway;
    private readonly UserGateway _userGateway;
    private readonly IVideoProcessingService _processingService;
    private readonly IStorageService _storageService;
    private readonly INotificationService _notificationService;

    private ProcessVideoUseCase(
        VideoGateway videoGateway,
        UserGateway userGateway,
        IVideoProcessingService processingService,
        IStorageService storageService,
        INotificationService notificationService)
    {
        _videoGateway = videoGateway;
        _userGateway = userGateway;
        _processingService = processingService;
        _storageService = storageService;
        _notificationService = notificationService;
    }

    public static ProcessVideoUseCase Create(
        VideoGateway videoGateway,
        UserGateway userGateway,
        IVideoProcessingService processingService,
        IStorageService storageService,
        INotificationService notificationService)
    {
        return new ProcessVideoUseCase(videoGateway, userGateway, processingService, storageService, notificationService);
    }

    public async Task<Video> Run(ProcessVideoCommand command)
    {
        var video = await _videoGateway.GetById(command.VideoId)
            ?? throw new Exception($"Vídeo {command.VideoId} não encontrado.");

        var user = await _userGateway.GetById(video.UserId)
            ?? throw new Exception($"Usuário {video.UserId} não encontrado.");

        video.StartProcessing();
        await _videoGateway.UpdateVideo(video);

        try
        {
            var (success, frameCount, zipContent, errorMessage) = await _processingService.ProcessVideoAsync(video.StoragePath);

            if (success && zipContent != null)
            {
                var zipPath = await _storageService.SaveZipAsync(zipContent, video.Id);
                video.CompleteProcessing(frameCount, zipPath);
                await _videoGateway.UpdateVideo(video);

                var downloadUrl = $"/api/videos/{video.Id}/download";
                await _notificationService.SendProcessingCompleteNotificationAsync(user.Email, video.OriginalFileName, downloadUrl);
            }
            else
            {
                video.FailProcessing(errorMessage ?? "Erro desconhecido no processamento.");
                await _videoGateway.UpdateVideo(video);

                await _notificationService.SendProcessingFailedNotificationAsync(user.Email, video.OriginalFileName, video.ErrorMessage!);
            }

            await _storageService.DeleteVideoAsync(video.StoragePath);
        }
        catch (Exception ex)
        {
            video.FailProcessing($"Erro no processamento: {ex.Message}");
            await _videoGateway.UpdateVideo(video);

            await _notificationService.SendProcessingFailedNotificationAsync(user.Email, video.OriginalFileName, video.ErrorMessage!);
        }

        return video;
    }
}
