using FiapX.Application.Controllers.Videos;
using FiapX.Application.Interfaces.DataSources;
using FiapX.Application.Interfaces.Services;
using FiapX.Domain.Enums;
using FiapX.Shared.DTO.Video.Input;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace FiapX.UnitTests.Controllers;

public class VideoControllerTests
{
    private readonly Mock<IVideoDataSource> _videoDataSourceMock;
    private readonly Mock<IUserDataSource> _userDataSourceMock;
    private readonly Mock<IStorageService> _storageServiceMock;
    private readonly Mock<IMessageQueueService> _messageQueueServiceMock;
    private readonly Mock<IVideoProcessingService> _processingServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly VideoController _controller;

    public VideoControllerTests()
    {
        _videoDataSourceMock = new Mock<IVideoDataSource>();
        _userDataSourceMock = new Mock<IUserDataSource>();
        _storageServiceMock = new Mock<IStorageService>();
        _messageQueueServiceMock = new Mock<IMessageQueueService>();
        _processingServiceMock = new Mock<IVideoProcessingService>();
        _notificationServiceMock = new Mock<INotificationService>();

        _controller = new VideoController(
            _videoDataSourceMock.Object,
            _userDataSourceMock.Object,
            _storageServiceMock.Object,
            _messageQueueServiceMock.Object,
            _processingServiceMock.Object,
            _notificationServiceMock.Object);
    }

    [Fact]
    public async Task UploadVideo_Deve_Retornar_Sucesso_Quando_Valido()
    {
        var userId = Guid.NewGuid();
        var videoId = Guid.NewGuid();
        var fileName = "video.mp4";
        var storagePath = $"/uploads/{videoId}.mp4";

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(1024);

        _storageServiceMock
            .Setup(s => s.SaveVideoAsync(It.IsAny<IFormFile>(), It.IsAny<Guid>()))
            .ReturnsAsync(storagePath);

        var result = await _controller.UploadVideo(userId, fileMock.Object);

        result.Succeeded.Should().BeTrue();
        result.Messages.Should().Contain("Vídeo enviado para processamento!");

        _videoDataSourceMock.Verify(x => x.Create(It.IsAny<VideoInputDto>()), Times.Once);
        _messageQueueServiceMock.Verify(x => x.PublishVideoForProcessingAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task GetVideosByUser_Deve_Retornar_Lista_Quando_Existir()
    {
        var userId = Guid.NewGuid();
        var videos = new List<VideoInputDto>
        {
            new(Guid.NewGuid(), userId, "video1.mp4", "/uploads/v1.mp4", VideoStatus.Completed, 10, "/outputs/v1.zip", null, DateTime.UtcNow, DateTime.UtcNow),
            new(Guid.NewGuid(), userId, "video2.mp4", "/uploads/v2.mp4", VideoStatus.Processing, null, null, null, DateTime.UtcNow, null)
        };

        _videoDataSourceMock.Setup(x => x.GetByUserId(userId)).ReturnsAsync(videos);

        var result = await _controller.GetVideosByUser(userId);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetVideosByUser_Deve_Retornar_Erro_Quando_Vazio()
    {
        var userId = Guid.NewGuid();
        _videoDataSourceMock.Setup(x => x.GetByUserId(userId)).ReturnsAsync(new List<VideoInputDto>());

        var result = await _controller.GetVideosByUser(userId);

        result.Succeeded.Should().BeFalse();
        result.Messages.Should().Contain("Nenhum vídeo encontrado.");
    }

    [Fact]
    public async Task GetVideoById_Deve_Retornar_Video_Quando_Encontrado()
    {
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var video = new VideoInputDto(videoId, userId, "video.mp4", "/uploads/v.mp4", VideoStatus.Completed, 10, "/outputs/v.zip", null, DateTime.UtcNow, DateTime.UtcNow);

        _videoDataSourceMock.Setup(x => x.GetById(videoId)).ReturnsAsync(video);

        var result = await _controller.GetVideoById(videoId, userId);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(videoId);
    }

    [Fact]
    public async Task GetVideoById_Deve_Retornar_Erro_Quando_Nao_Encontrado()
    {
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _videoDataSourceMock.Setup(x => x.GetById(videoId)).ReturnsAsync((VideoInputDto?)null);

        var result = await _controller.GetVideoById(videoId, userId);

        result.Succeeded.Should().BeFalse();
        result.Messages.Should().Contain("Vídeo não encontrado.");
    }

    [Fact]
    public async Task GetVideoById_Deve_Retornar_Erro_Quando_Usuario_Diferente()
    {
        var videoId = Guid.NewGuid();
        var videoUserId = Guid.NewGuid();
        var requestUserId = Guid.NewGuid();

        var video = new VideoInputDto(videoId, videoUserId, "video.mp4", "/uploads/v.mp4", VideoStatus.Completed, 10, null, null, DateTime.UtcNow, null);

        _videoDataSourceMock.Setup(x => x.GetById(videoId)).ReturnsAsync(video);

        var result = await _controller.GetVideoById(videoId, requestUserId);

        result.Succeeded.Should().BeFalse();
        result.Messages.Should().Contain("Acesso não autorizado a este vídeo.");
    }

    [Fact]
    public async Task DownloadVideo_Deve_Retornar_Sucesso_Quando_Disponivel()
    {
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var zipPath = "/outputs/frames.zip";
        var zipContent = new byte[] { 1, 2, 3 };

        var video = new VideoInputDto(videoId, userId, "video.mp4", "/uploads/v.mp4", VideoStatus.Completed, 10, zipPath, null, DateTime.UtcNow, DateTime.UtcNow);

        _videoDataSourceMock.Setup(x => x.GetById(videoId)).ReturnsAsync(video);
        _storageServiceMock.Setup(x => x.GetZipAsync(zipPath)).ReturnsAsync(zipContent);

        var (success, content, fileName, errorMessage) = await _controller.DownloadVideo(videoId, userId);

        success.Should().BeTrue();
        content.Should().BeEquivalentTo(zipContent);
        fileName.Should().Contain("frames_");
    }

    [Fact]
    public async Task DownloadVideo_Deve_Retornar_Erro_Quando_Zip_Nao_Disponivel()
    {
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var video = new VideoInputDto(videoId, userId, "video.mp4", "/uploads/v.mp4", VideoStatus.Processing, null, null, null, DateTime.UtcNow, null);

        _videoDataSourceMock.Setup(x => x.GetById(videoId)).ReturnsAsync(video);

        var (success, content, fileName, errorMessage) = await _controller.DownloadVideo(videoId, userId);

        success.Should().BeFalse();
        errorMessage.Should().Contain("ZIP ainda não está disponível");
    }
}
