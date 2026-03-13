using System.Diagnostics.CodeAnalysis;

namespace FiapX.API.Extensions.Middlewares;

[ExcludeFromCodeCoverage]
public class UnauthorizedTokenMiddleware : IMiddleware
{
    private readonly ILogger<UnauthorizedTokenMiddleware> _logger;

    public UnauthorizedTokenMiddleware(ILogger<UnauthorizedTokenMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await next(context);

        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
        {
            _logger.LogWarning("Acesso não autorizado detectado para: {Path}", context.Request.Path);
        }
    }
}
