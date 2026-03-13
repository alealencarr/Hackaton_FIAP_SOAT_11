using FiapX.Application.Controllers.Users;
using FiapX.Application.Interfaces.DataSources;
using FiapX.Application.Interfaces.Services;
using FiapX.Infrastructure.DataSources;
using FiapX.Infrastructure.DbContexts;
using FiapX.Shared.DTO.User.Output;
using FiapX.Shared.DTO.User.Request;
using FiapX.Shared.Result;
using Microsoft.AspNetCore.Mvc;
using MiniValidation;
using System.Diagnostics.CodeAnalysis;

namespace FiapX.API.Endpoints.Auth;

[ExcludeFromCodeCoverage]
internal sealed class Register : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/register",
            async (
                AppDbContext appDbContext,
                IJwtService jwtService,
                [FromBody] UserRegisterRequestDto request) =>
            {
                if (!MiniValidator.TryValidate(request, out var errors))
                    return Results.ValidationProblem(errors);

                IUserDataSource dataSource = new UserDataSource(appDbContext);
                var controller = new UserController(dataSource, jwtService);

                var result = await controller.Register(request);

                return result.Succeeded 
                    ? Results.Created($"/api/users/{result.Data?.Id}", result) 
                    : Results.BadRequest(result);
            })
            .WithTags("Auth")
            .Produces<ICommandResult<UserOutputDto>>()
            .WithName("Auth.Register")
            .AllowAnonymous();
    }
}
