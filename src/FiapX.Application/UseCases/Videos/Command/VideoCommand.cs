using Microsoft.AspNetCore.Http;

namespace FiapX.Application.UseCases.Videos.Command;

public class UploadVideoCommand
{
    public UploadVideoCommand(Guid userId, IFormFile videoFile)
    {
        UserId = userId;
        VideoFile = videoFile;
    }

    public UploadVideoCommand() { }

    public Guid UserId { get; set; }
    public IFormFile VideoFile { get; set; } = null!;
}

public class ProcessVideoCommand
{
    public ProcessVideoCommand(Guid videoId)
    {
        VideoId = videoId;
    }

    public ProcessVideoCommand() { }

    public Guid VideoId { get; set; }
}
