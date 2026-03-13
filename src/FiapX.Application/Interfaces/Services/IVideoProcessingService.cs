namespace FiapX.Application.Interfaces.Services;

public interface IVideoProcessingService
{
    Task<(bool Success, int FrameCount, byte[]? ZipContent, string? ErrorMessage)> ProcessVideoAsync(string videoPath);
}
