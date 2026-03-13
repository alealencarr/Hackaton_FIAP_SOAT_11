using FiapX.Application.Controllers.Videos;
using FiapX.Application.Interfaces.DataSources;
using FiapX.Application.Interfaces.Services;
using FiapX.Infrastructure.DataSources;
using FiapX.Infrastructure.DbContexts;
using FiapX.Shared.DTO.Video.Output;
using FiapX.Shared.Result;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace FiapX.API.Endpoints.Videos;

[ExcludeFromCodeCoverage]
internal sealed class Upload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/videos/upload",
            async (
                AppDbContext appDbContext,
                IStorageService storageService,
                IMessageQueueService messageQueueService,
                IVideoProcessingService processingService,
                INotificationService notificationService,
                HttpContext httpContext,
                IFormFile videoFile) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return Results.Unauthorized();

                const long maxFileSize = 30 * 1024 * 1024; 
                if (videoFile.Length > maxFileSize)
                    return Results.BadRequest(CommandResult.Fail("O arquivo excede o limite de 30MB."));

                var allowedExtensions = new[] { ".mp4", ".avi", ".mov", ".mkv", ".wmv", ".flv", ".webm" };
                var extension = Path.GetExtension(videoFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                    return Results.BadRequest(CommandResult.Fail("Formato de vídeo não suportado. Use: mp4, avi, mov, mkv, wmv, flv ou webm."));

                IVideoDataSource videoDataSource = new VideoDataSource(appDbContext);
                IUserDataSource userDataSource = new UserDataSource(appDbContext);

                var controller = new VideoController(
                    videoDataSource,
                    userDataSource,
                    storageService,
                    messageQueueService,
                    processingService,
                    notificationService);

                var result = await controller.UploadVideo(userId, videoFile);

                return result.Succeeded 
                    ? Results.Created($"/api/videos/{result.Data?.Id}", result) 
                    : Results.BadRequest(result);
            })
            .WithTags("Videos")
            .Produces<ICommandResult<VideoOutputDto>>()
            .WithName("Video.Upload")
            .DisableAntiforgery()
            .RequireAuthorization();
    }
}
