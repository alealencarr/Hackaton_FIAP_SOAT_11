using FiapX.Domain.Entities;
using FiapX.Shared.DTO.Video.Output;
using FiapX.Shared.Result;

namespace FiapX.Application.Presenter.Videos;

public class VideoPresenter
{
    private readonly string _message;
    private readonly string _baseUrl;

    public VideoPresenter(string? message = null, string baseUrl = "")
    {
        _message = message ?? string.Empty;
        _baseUrl = baseUrl;
    }

    public ICommandResult<VideoOutputDto> TransformObject(Video video)
    {
        return CommandResult<VideoOutputDto>.Success(Transform(video), _message);
    }

    public ICommandResult<List<VideoOutputDto>> TransformList(List<Video> videos)
    {
        return CommandResult<List<VideoOutputDto>>.Success(videos.Select(Transform).ToList(), _message);
    }

    public VideoOutputDto Transform(Video video)
    {
        return new VideoOutputDto
        {
            Id = video.Id,
            UserId = video.UserId,
            OriginalFileName = video.OriginalFileName,
            Status = video.Status.ToString(),
            FrameCount = video.FrameCount,
            DownloadUrl = video.ZipPath != null ? $"{_baseUrl}/api/videos/{video.Id}/download" : null,
            ErrorMessage = video.ErrorMessage,
            CreatedAt = video.CreatedAt,
            ProcessedAt = video.ProcessedAt
        };
    }

    public ICommandResult<T> Error<T>(string message)
    {
        return CommandResult<T>.Fail(message);
    }
}
