using System.Diagnostics.CodeAnalysis;
using FiapX.Domain.Enums;

namespace FiapX.Shared.DTO.Video.Input;

[ExcludeFromCodeCoverage]
public record VideoInputDto(
    Guid Id, 
    Guid UserId, 
    string OriginalFileName, 
    string StoragePath, 
    VideoStatus Status, 
    int? FrameCount, 
    string? ZipPath, 
    string? ErrorMessage, 
    DateTime CreatedAt, 
    DateTime? ProcessedAt
);
