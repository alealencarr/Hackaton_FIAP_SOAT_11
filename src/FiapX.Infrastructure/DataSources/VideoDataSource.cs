using FiapX.Application.Interfaces.DataSources;
using FiapX.Domain.Enums;
using FiapX.Infrastructure.DbContexts;
using FiapX.Infrastructure.DbModels;
using FiapX.Shared.DTO.Video.Input;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace FiapX.Infrastructure.DataSources;

[ExcludeFromCodeCoverage]
public class VideoDataSource : IVideoDataSource
{
    private readonly AppDbContext _appDbContext;

    public VideoDataSource(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task Create(VideoInputDto video)
    {
        var videoDbModel = new VideoDbModel(
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

        await _appDbContext.AddAsync(videoDbModel);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task Update(VideoInputDto video)
    {
        var videoDb = await _appDbContext.Videos
            .Where(x => x.Id == video.Id)
            .FirstOrDefaultAsync() ?? throw new Exception("Vídeo não encontrado.");

        videoDb.Status = video.Status;
        videoDb.FrameCount = video.FrameCount;
        videoDb.ZipPath = video.ZipPath;
        videoDb.ErrorMessage = video.ErrorMessage;
        videoDb.ProcessedAt = video.ProcessedAt;

        _appDbContext.Update(videoDb);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<VideoInputDto?> GetById(Guid id)
    {
        var video = await _appDbContext.Videos
            .AsNoTracking()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        return video is not null ? MapToDto(video) : null;
    }

    public async Task<List<VideoInputDto>> GetByUserId(Guid userId)
    {
        var videos = await _appDbContext.Videos
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return videos.Select(MapToDto).ToList();
    }

    public async Task<List<VideoInputDto>> GetPendingVideos()
    {
        var videos = await _appDbContext.Videos
            .AsNoTracking()
            .Where(x => x.Status == VideoStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        return videos.Select(MapToDto).ToList();
    }

    public async Task<List<VideoInputDto>> GetAll()
    {
        var videos = await _appDbContext.Videos
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return videos.Select(MapToDto).ToList();
    }

    private static VideoInputDto MapToDto(VideoDbModel video)
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
