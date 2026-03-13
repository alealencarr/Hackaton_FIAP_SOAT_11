using FiapX.Application.Gateways;
using FiapX.Application.Interfaces.DataSources;
using FiapX.Application.Interfaces.Services;
using FiapX.Application.Presenter.Videos;
using FiapX.Application.UseCases.Videos;
using FiapX.Application.UseCases.Videos.Command;
using FiapX.Shared.DTO.Video.Output;
using FiapX.Shared.Result;
using Microsoft.AspNetCore.Http;

namespace FiapX.Application.Controllers.Videos;

public class VideoController
{
    private readonly IVideoDataSource _videoDataSource;
    private readonly IUserDataSource _userDataSource;
    private readonly IStorageService _storageService;
    private readonly IMessageQueueService _messageQueueService;
    private readonly IVideoProcessingService _processingService;
    private readonly INotificationService _notificationService;

    public VideoController(
        IVideoDataSource videoDataSource,
        IUserDataSource userDataSource,
        IStorageService storageService,
        IMessageQueueService messageQueueService,
        IVideoProcessingService processingService,
        INotificationService notificationService)
    {
        _videoDataSource = videoDataSource;
        _userDataSource = userDataSource;
        _storageService = storageService;
        _messageQueueService = messageQueueService;
        _processingService = processingService;
        _notificationService = notificationService;
    }

    public async Task<ICommandResult<VideoOutputDto>> UploadVideo(Guid userId, IFormFile videoFile)
    {
        var presenter = new VideoPresenter("Vídeo enviado para processamento!");

        try
        {
            var command = new UploadVideoCommand(userId, videoFile);

            var videoGateway = VideoGateway.Create(_videoDataSource);
            var useCase = UploadVideoUseCase.Create(videoGateway, _storageService, _messageQueueService);
            var video = await useCase.Run(command);

            return presenter.TransformObject(video);
        }
        catch (Exception ex)
        {
            return presenter.Error<VideoOutputDto>(ex.Message);
        }
    }

    public async Task<ICommandResult<List<VideoOutputDto>>> GetVideosByUser(Guid userId)
    {
        var presenter = new VideoPresenter("Vídeos encontrados!");

        try
        {
            var videoGateway = VideoGateway.Create(_videoDataSource);
            var useCase = GetVideosByUserUseCase.Create(videoGateway);
            var videos = await useCase.Run(userId);

            return videos.Count == 0 
                ? presenter.Error<List<VideoOutputDto>>("Nenhum vídeo encontrado.") 
                : presenter.TransformList(videos);
        }
        catch (Exception ex)
        {
            return presenter.Error<List<VideoOutputDto>>(ex.Message);
        }
    }

    public async Task<ICommandResult<VideoOutputDto?>> GetVideoById(Guid id, Guid userId)
    {
        var presenter = new VideoPresenter("Vídeo encontrado!");

        try
        {
            var videoGateway = VideoGateway.Create(_videoDataSource);
            var useCase = GetVideoByIdUseCase.Create(videoGateway);
            var video = await useCase.Run(id);

            if (video is null)
                return presenter.Error<VideoOutputDto?>("Vídeo não encontrado.");

            if (video.UserId != userId)
                return presenter.Error<VideoOutputDto?>("Acesso não autorizado a este vídeo.");

            return presenter.TransformObject(video);
        }
        catch (Exception ex)
        {
            return presenter.Error<VideoOutputDto?>(ex.Message);
        }
    }

    public async Task<(bool Success, byte[]? Content, string? FileName, string? ErrorMessage)> DownloadVideo(Guid id, Guid userId)
    {
        try
        {
            var videoGateway = VideoGateway.Create(_videoDataSource);
            var useCase = GetVideoByIdUseCase.Create(videoGateway);
            var video = await useCase.Run(id);

            if (video is null)
                return (false, null, null, "Vídeo não encontrado.");

            if (video.UserId != userId)
                return (false, null, null, "Acesso não autorizado a este vídeo.");

            if (string.IsNullOrEmpty(video.ZipPath))
                return (false, null, null, "Arquivo ZIP ainda não está disponível.");

            var content = await _storageService.GetZipAsync(video.ZipPath);
            if (content is null)
                return (false, null, null, "Arquivo não encontrado no storage.");

            var fileName = $"frames_{video.OriginalFileName.Replace(Path.GetExtension(video.OriginalFileName), "")}.zip";

            return (true, content, fileName, null);
        }
        catch (Exception ex)
        {
            return (false, null, null, ex.Message);
        }
    }

    public async Task ProcessVideo(Guid videoId)
    {
        try
        {
            var videoGateway = VideoGateway.Create(_videoDataSource);
            var userGateway = UserGateway.Create(_userDataSource);

            var useCase = ProcessVideoUseCase.Create(
                videoGateway,
                userGateway,
                _processingService,
                _storageService,
                _notificationService);

            var command = new ProcessVideoCommand(videoId);
            await useCase.Run(command);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
