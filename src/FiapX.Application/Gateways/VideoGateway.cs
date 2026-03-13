using FiapX.Application.Interfaces.DataSources;
using FiapX.Domain.Entities;
using FiapX.Domain.Enums;
using FiapX.Shared.DTO.Video.Input;

namespace FiapX.Application.Gateways;

public class VideoGateway
{
    private readonly IVideoDataSource _dataSource;

    private VideoGateway(IVideoDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public static VideoGateway Create(IVideoDataSource dataSource)
    {
        return new VideoGateway(dataSource);
    }

    public async Task<List<Video>> GetByUserId(Guid userId)
    {
        var videos = await _dataSource.GetByUserId(userId);
        return videos.Select(MapToEntity).ToList();
    }

    public async Task<List<Video>> GetPendingVideos()
    {
        var videos = await _dataSource.GetPendingVideos();
        return videos.Select(MapToEntity).ToList();
    }

    public async Task<Video?> GetById(Guid id)
    {
        var video = await _dataSource.GetById(id);
        return video is not null ? MapToEntity(video) : null;
    }

    public async Task CreateVideo(Video video)
    {
        var videoInput = MapToDto(video);
        await _dataSource.Create(videoInput);
    }

    public async Task UpdateVideo(Video video)
    {
        var videoInput = MapToDto(video);
        await _dataSource.Update(videoInput);
    }

    private static Video MapToEntity(VideoInputDto dto)
    {
        return new Video(
            dto.Id,
            dto.UserId,
            dto.OriginalFileName,
            dto.StoragePath,
            dto.Status,
            dto.FrameCount,
            dto.ZipPath,
            dto.ErrorMessage,
            dto.CreatedAt,
            dto.ProcessedAt
        );
    }

    private static VideoInputDto MapToDto(Video video)
    {
        return new VideoInputDto(
            video.Id,
            video.UserId,
            video.OriginalFileName,
            video.StoragePath,
            video.Status,
            video.FrameCount,
            video.ZipPath,
            video.ErrorMessage,
            video.CreatedAt,
            video.ProcessedAt
        );
    }
}
