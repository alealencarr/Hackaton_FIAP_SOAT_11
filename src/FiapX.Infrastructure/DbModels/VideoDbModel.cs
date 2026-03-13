using System.Diagnostics.CodeAnalysis;
using FiapX.Domain.Enums;

namespace FiapX.Infrastructure.DbModels;

[ExcludeFromCodeCoverage]
public class VideoDbModel
{
    public VideoDbModel(Guid id, Guid userId, string originalFileName, string storagePath, 
        VideoStatus status, int? frameCount, string? zipPath, string? errorMessage, 
        DateTime createdAt, DateTime? processedAt)
    {
        Id = id;
        UserId = userId;
        OriginalFileName = originalFileName;
        StoragePath = storagePath;
        Status = status;
        FrameCount = frameCount;
        ZipPath = zipPath;
        ErrorMessage = errorMessage;
        CreatedAt = createdAt;
        ProcessedAt = processedAt;
    }

    public VideoDbModel() { }

    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public VideoStatus Status { get; set; }
    public int? FrameCount { get; set; }
    public string? ZipPath { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public UserDbModel User { get; set; } = null!;
}
