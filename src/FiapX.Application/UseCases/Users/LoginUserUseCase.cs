using FiapX.Application.Gateways;
using FiapX.Application.Interfaces.Services;
using FiapX.Application.UseCases.Users.Command;
using FiapX.Domain.Entities;

namespace FiapX.Application.UseCases.Users;

public class LoginUserUseCase
{
    private readonly UserGateway _gateway;
    private readonly IJwtService _jwtService;

    private LoginUserUseCase(UserGateway gateway, IJwtService jwtService)
    {
        _gateway = gateway;
        _jwtService = jwtService;
    }

    public static LoginUserUseCase Create(UserGateway gateway, IJwtService jwtService)
    {
        return new LoginUserUseCase(gateway, jwtService);
    }

    public async Task<(User User, string Token, DateTime ExpiresAt)> Run(LoginUserCommand command)
    {
        try
        {
            var user = await _gateway.GetByEmail(command.Email)
                ?? throw new ArgumentException("E-mail ou senha inválidos.");

            if (!user.ValidatePassword(command.Password))
                throw new ArgumentException("E-mail ou senha inválidos.");

            var (token, expiresAt) = _jwtService.GenerateToken(user);

            return (user, token, expiresAt);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao realizar login: {ex.Message}");
        }
    }
}
