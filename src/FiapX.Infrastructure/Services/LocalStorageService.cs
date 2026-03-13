using FiapX.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace FiapX.Infrastructure.Services;

[ExcludeFromCodeCoverage]
public class LocalStorageService : IStorageService
{
    private readonly string _uploadPath;
    private readonly string _outputPath;

    public LocalStorageService(IConfiguration configuration)
    {
        _uploadPath = configuration["Storage:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _outputPath = configuration["Storage:OutputPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "outputs");

        Directory.CreateDirectory(_uploadPath);
        Directory.CreateDirectory(_outputPath);
    }

    public async Task<string> SaveVideoAsync(IFormFile file, Guid videoId)
    {
        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{videoId}{extension}";
        var filePath = Path.Combine(_uploadPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return filePath;
    }

    public async Task<string> SaveZipAsync(byte[] zipContent, Guid videoId)
    {
        var fileName = $"frames_{videoId}.zip";
        var filePath = Path.Combine(_outputPath, fileName);

        await File.WriteAllBytesAsync(filePath, zipContent);

        return filePath;
    }

    public async Task<byte[]?> GetZipAsync(string zipPath)
    {
        if (!File.Exists(zipPath))
            return null;

        return await File.ReadAllBytesAsync(zipPath);
    }

    public async Task DeleteVideoAsync(string path)
    {
        if (File.Exists(path))
        {
            await Task.Run(() => File.Delete(path));
        }
    }

    public string GetVideoPath(Guid videoId)
    {
        var files = Directory.GetFiles(_uploadPath, $"{videoId}.*");
        return files.FirstOrDefault() ?? string.Empty;
    }
}
