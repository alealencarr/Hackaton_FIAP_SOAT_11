using FiapX.Application.Gateways;
using FiapX.Application.UseCases.Users.Command;
using FiapX.Domain.Entities;

namespace FiapX.Application.UseCases.Users;

public class RegisterUserUseCase
{
    private readonly UserGateway _gateway;

    private RegisterUserUseCase(UserGateway gateway)
    {
        _gateway = gateway;
    }

    public static RegisterUserUseCase Create(UserGateway gateway)
    {
        return new RegisterUserUseCase(gateway);
    }

    public async Task<User> Run(RegisterUserCommand command)
    {
        try
        {
            var existingUser = await _gateway.ExistsByEmail(command.Email);
            if (existingUser)
                throw new ArgumentException("Já existe um usuário cadastrado com este e-mail.");

            var user = new User(command.Name, command.Email, command.Password);
            await _gateway.CreateUser(user);

            return user;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao registrar usuário: {ex.Message}");
        }
    }
}
