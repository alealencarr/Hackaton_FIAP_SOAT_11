using FiapX.Application.Interfaces.DataSources;
using FiapX.Application.Interfaces.Services;
using FiapX.Infrastructure.DataSources;
using FiapX.Infrastructure.DbContexts;
using FiapX.Infrastructure.Persistence;
using FiapX.Infrastructure.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace FiapX.Infrastructure;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddServices(configuration);
        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(Configuration.ConnectionString) 
            ?? configuration.GetConnectionString("Default");

        services.AddTransient<DataSeeder>();

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null
                );
            });
        });

        services.AddHealthChecksUI(setup =>
        {
            setup.SetEvaluationTimeInSeconds(10);
            setup.MaximumHistoryEntriesPerEndpoint(50);
        }).AddInMemoryStorage();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IVideoDataSource, VideoDataSource>();
        services.AddScoped<IUserDataSource, UserDataSource>();
        services.AddScoped<IStorageService, LocalStorageService>();
        services.AddScoped<IVideoProcessingService, FFmpegVideoProcessingService>();
        services.AddScoped<INotificationService, EmailNotificationService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddSingleton<IMessageQueueService, RabbitMQService>();

        return services;
    }

    public static IHealthChecksBuilder AddHealthDb(this IHealthChecksBuilder services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(Configuration.ConnectionString);
        services.AddSqlServer(connectionString!, name: "SQL Server Check", tags: new[] { "db", "data" });
        return services;
    }

    public static void AddHealthChecks(this WebApplication app)
    {
        app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.UseHealthChecksUI(options => { options.UIPath = "/dashboard"; });
    }
}
