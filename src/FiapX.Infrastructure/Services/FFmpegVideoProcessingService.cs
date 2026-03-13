using FiapX.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

namespace FiapX.Infrastructure.Services;

[ExcludeFromCodeCoverage]
public class FFmpegVideoProcessingService : IVideoProcessingService
{
    private readonly ILogger<FFmpegVideoProcessingService> _logger;

    public FFmpegVideoProcessingService(ILogger<FFmpegVideoProcessingService> logger)
    {
        _logger = logger;
    }

    public async Task<(bool Success, int FrameCount, byte[]? ZipContent, string? ErrorMessage)> ProcessVideoAsync(string videoPath)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"fiapx_{Guid.NewGuid()}");
        
        try
        {
            Directory.CreateDirectory(tempDir);
            _logger.LogInformation("Iniciando processamento do vídeo: {VideoPath}", videoPath);

            var framePattern = Path.Combine(tempDir, "frame_%04d.png");

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{videoPath}\" -vf fps=1 -y \"{framePattern}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                return (false, 0, null, "Não foi possível iniciar o FFmpeg.");
            }

            var errorOutput = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                _logger.LogError("FFmpeg falhou com código de saída {ExitCode}: {Error}", process.ExitCode, errorOutput);
                return (false, 0, null, $"Erro no FFmpeg: {errorOutput}");
            }

            var frames = Directory.GetFiles(tempDir, "*.png");
            if (frames.Length == 0)
            {
                return (false, 0, null, "Nenhum frame foi extraído do vídeo.");
            }

            _logger.LogInformation("Extraídos {FrameCount} frames", frames.Length);

            var zipContent = CreateZipFromFrames(frames);

            return (true, frames.Length, zipContent, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar vídeo");
            return (false, 0, null, ex.Message);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Não foi possível limpar diretório temporário: {TempDir}", tempDir);
                }
            }
        }
    }

    private static byte[] CreateZipFromFrames(string[] frames)
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var frame in frames)
            {
                var entryName = Path.GetFileName(frame);
                var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                
                using var entryStream = entry.Open();
                using var fileStream = File.OpenRead(frame);
                fileStream.CopyTo(entryStream);
            }
        }

        return memoryStream.ToArray();
    }
}
