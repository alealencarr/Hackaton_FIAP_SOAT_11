using System.Diagnostics.CodeAnalysis;

namespace FiapX.Shared.DTO.Video.Output;

[ExcludeFromCodeCoverage]
public record VideoOutputDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? FrameCount { get; set; }
    public string? DownloadUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
