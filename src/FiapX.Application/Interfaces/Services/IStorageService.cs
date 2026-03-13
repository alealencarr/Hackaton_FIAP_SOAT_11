using Microsoft.AspNetCore.Http;

namespace FiapX.Application.Interfaces.Services;

public interface IStorageService
{
    Task<string> SaveVideoAsync(IFormFile file, Guid videoId);
    Task<string> SaveZipAsync(byte[] zipContent, Guid videoId);
    Task<byte[]?> GetZipAsync(string zipPath);
    Task DeleteVideoAsync(string path);
    string GetVideoPath(Guid videoId);
}
