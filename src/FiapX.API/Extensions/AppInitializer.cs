using FiapX.Infrastructure.DbContexts;
using FiapX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace FiapX.API.Extensions;

[ExcludeFromCodeCoverage]
internal static class AppInitializer
{
    internal static async Task<IApplicationBuilder> InitializeApp(this WebApplication app, Serilog.ILogger logger)
    {
        await app.InitializeDatabase(logger);
        return app;
    }

    private static async Task<IApplicationBuilder> InitializeDatabase(this IApplicationBuilder app, Serilog.ILogger logger)
    {
        logger.Information("Iniciando inicialização do banco de dados...");
        
        using var serviceScope = app.ApplicationServices.CreateScope();
        var initializer = serviceScope.ServiceProvider.GetRequiredService<DataSeeder>();
        var appDbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

        await appDbContext.Database.MigrateAsync();
        await initializer.Initialize();
        
        logger.Information("Banco de dados inicializado com sucesso.");
        return await Task.FromResult(app);
    }
}
