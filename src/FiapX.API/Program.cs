using FiapX.API;
using FiapX.API.Extensions;
using FiapX.Infrastructure;
using Microsoft.AspNetCore.Http.Features;
using Prometheus;
using Serilog;

Log.Logger = LogExtensions.ConfigureLog();

try
{
    Log.Information("Iniciando FIAP X - Sistema de Processamento de Vídeos...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services
        .AddPresentation(builder.Configuration)
        .AddInfrastructure(builder.Configuration)
        .AddHealthChecks()
        .AddHealthApi()
        .AddHealthDb(builder.Configuration);

    //builder.Services.Configure<FormOptions>(options =>
    //{
    //    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
    //});

    //builder.WebHost.ConfigureKestrel(options =>
    //{
    //    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
    //});

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend",
            policy => policy
                .WithOrigins("https://hackatonalealencarr.vercel.app")
                .AllowAnyHeader()
                .AllowAnyMethod());
    });

    var app = builder.Build();
    app.UseHttpMetrics();
    await app.InitializeApp(Log.Logger);

    app.RegisterPipeline();
    app.AddHealthChecks();



    app.UseCors("AllowFrontend");


    app.MapGet("/", () => Results.Ok("FIAP X - Sistema de Processamento de Vídeos - Running"))
        .WithTags("Status")
        .WithName("Home");
    app.MapMetrics();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação terminou inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
