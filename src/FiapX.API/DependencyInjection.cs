using FiapX.API.Extensions;
using FiapX.API.Extensions.HealthCheck;
using FiapX.API.Extensions.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace FiapX.API;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(configuration);
        services.AddAuthorization();
        services.AddSerilog();

        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

        services.AddControllersWithViews();

        services.AddCors(opt =>
        {
            opt.AddDefaultPolicy(builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        services.AddOpenApi("v1", options => 
        { 
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); 
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddTransient<UnauthorizedTokenMiddleware>();
        services.AddEndpoints(Assembly.GetExecutingAssembly());

        services.AddHttpContextAccessor();

        services.AddSwaggerGen(x =>
        {
            x.CustomSchemaIds(n => n.FullName);
            x.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "FIAP X - Sistema de Processamento de Vídeos", 
                Version = "v1", 
                Description = "API para processamento de vídeos e extração de frames" 
            });

            var securitySchema = new OpenApiSecurityScheme
            {
                Description = "Autorização via JWT token (Digite 'Bearer {seu_token}' para autenticar).",
                Name = "Authorization",
                In = ParameterLocation.Header,
                BearerFormat = "JWT",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            x.AddSecurityDefinition("Bearer", securitySchema);
            x.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securitySchema, new[] { "Bearer" } }
            });
        });

        services.AddTransient<ApiHealthCheck>();

        return services;
    }

    public static IHealthChecksBuilder AddHealthApi(this IHealthChecksBuilder services)
    {
        services.AddCheck<ApiHealthCheck>("API")
            .AddPrivateMemoryHealthCheck(
                maximumMemoryBytes: 2_000_000_000,
                name: "Uso de Memória",
                failureStatus: HealthStatus.Degraded);
        return services;
    }

    public static void RegisterPipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.UseExceptionHandler(o => { });
        app.UseMiddleware<UnauthorizedTokenMiddleware>();

        if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
        {
            app.AddApiDocumentation();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors(builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapEndpoints();
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(opt =>
        {
            opt.SaveToken = true;
            opt.RequireHttpsMetadata = false;
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.Zero,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
            };
        });

        return services;
    }

    public static void AddApiDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.AddScalar();
    }

    public static void AddScalar(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("FIAP X - Sistema de Processamento de Vídeos")
                .AddPreferredSecuritySchemes("Bearer")
                .AddHttpAuthentication("Bearer", auth =>
                {
                    auth.Token = "seu_token_aqui";
                });
        });
    }
}
