using FiapX.Application.Controllers.Videos;
using FiapX.Application.Interfaces.DataSources;
using FiapX.Application.Interfaces.Services;
using FiapX.Infrastructure.DataSources;
using FiapX.Infrastructure.DbContexts;
using FiapX.Shared.Result;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace FiapX.API.Endpoints.Videos;

[ExcludeFromCodeCoverage]
internal sealed class Download : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/videos/{id:guid}/download",
            async (
                Guid id,
                AppDbContext appDbContext,
                IStorageService storageService,
                IMessageQueueService messageQueueService,
                IVideoProcessingService processingService,
                INotificationService notificationService,
                HttpContext httpContext) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return Results.Unauthorized();

                IVideoDataSource videoDataSource = new VideoDataSource(appDbContext);
                IUserDataSource userDataSource = new UserDataSource(appDbContext);

                var controller = new VideoController(
                    videoDataSource,
                    userDataSource,
                    storageService,
                    messageQueueService,
                    processingService,
                    notificationService);

                var (success, content, fileName, errorMessage) = await controller.DownloadVideo(id, userId);

                if (!success)
                    return Results.NotFound(CommandResult.Fail(errorMessage!));

                return Results.File(content!, "application/zip", fileName);
            })
            .WithTags("Videos")
            .Produces<byte[]>(contentType: "application/zip")
            .WithName("Video.Download")
            .RequireAuthorization();
    }
}
