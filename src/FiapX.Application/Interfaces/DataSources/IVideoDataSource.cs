using FiapX.Shared.DTO.Video.Input;

namespace FiapX.Application.Interfaces.DataSources;

public interface IVideoDataSource
{
    Task Create(VideoInputDto video);
    Task Update(VideoInputDto video);
    Task<VideoInputDto?> GetById(Guid id);
    Task<List<VideoInputDto>> GetByUserId(Guid userId);
    Task<List<VideoInputDto>> GetPendingVideos();
    Task<List<VideoInputDto>> GetAll();
}
