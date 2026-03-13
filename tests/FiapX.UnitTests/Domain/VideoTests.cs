using FiapX.Domain.Entities;
using FiapX.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace FiapX.UnitTests.Domain;

public class VideoTests
{
    [Fact]
    public void Deve_Criar_Video_Valido()
    {
        var userId = Guid.NewGuid();
        var fileName = "video.mp4";
        var storagePath = "/uploads/video.mp4";

        var video = new Video(userId, fileName, storagePath);

        video.Should().NotBeNull();
        video.Id.Should().NotBeEmpty();
        video.UserId.Should().Be(userId);
        video.OriginalFileName.Should().Be(fileName);
        video.StoragePath.Should().Be(storagePath);
        video.Status.Should().Be(VideoStatus.Pending);
        video.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_UserId_Vazio()
    {
        Action act = () => new Video(Guid.Empty, "video.mp4", "/uploads/video.mp4");

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*UserId*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_FileName_Invalido(string fileName)
    {
        Action act = () => new Video(Guid.NewGuid(), fileName, "/uploads/video.mp4");

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*nome do arquivo*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_StoragePath_Invalido(string storagePath)
    {
        Action act = () => new Video(Guid.NewGuid(), "video.mp4", storagePath);

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*caminho*");
    }

    [Fact]
    public void Deve_Iniciar_Processamento()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "/uploads/video.mp4");

        video.StartProcessing();

        video.Status.Should().Be(VideoStatus.Processing);
    }

    [Fact]
    public void Deve_Completar_Processamento()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "/uploads/video.mp4");
        video.StartProcessing();

        video.CompleteProcessing(10, "/outputs/frames.zip");

        video.Status.Should().Be(VideoStatus.Completed);
        video.FrameCount.Should().Be(10);
        video.ZipPath.Should().Be("/outputs/frames.zip");
        video.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deve_Falhar_Processamento()
    {
        var video = new Video(Guid.NewGuid(), "video.mp4", "/uploads/video.mp4");
        video.StartProcessing();

        video.FailProcessing("Erro no FFmpeg");

        video.Status.Should().Be(VideoStatus.Failed);
        video.ErrorMessage.Should().Be("Erro no FFmpeg");
        video.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deve_Reconstituir_Video_Completo()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var processedAt = DateTime.UtcNow;

        var video = new Video(id, userId, "video.mp4", "/uploads/video.mp4", 
            VideoStatus.Completed, 10, "/outputs/frames.zip", null, createdAt, processedAt);

        video.Id.Should().Be(id);
        video.UserId.Should().Be(userId);
        video.Status.Should().Be(VideoStatus.Completed);
        video.FrameCount.Should().Be(10);
    }
}
