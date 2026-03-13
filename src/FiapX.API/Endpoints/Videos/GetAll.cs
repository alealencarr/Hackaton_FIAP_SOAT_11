using FiapX.Application.Controllers.Videos;
using FiapX.Application.Interfaces.DataSources;
using FiapX.Application.Interfaces.Services;
using FiapX.Infrastructure.DataSources;
using FiapX.Infrastructure.DbContexts;
using FiapX.Shared.DTO.Video.Output;
using FiapX.Shared.Result;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace FiapX.API.Endpoints.Videos;

[ExcludeFromCodeCoverage]
internal sealed class GetAll : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/videos",
            async (
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

                var result = await controller.GetVideosByUser(userId);

                return result.Succeeded 
                    ? Results.Ok(result) 
                    : Results.NotFound(result);
            })
            .WithTags("Videos")
            .Produces<ICommandResult<List<VideoOutputDto>>>()
            .WithName("Video.GetAll")
            .RequireAuthorization();
    }
}
