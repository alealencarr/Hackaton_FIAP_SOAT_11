using FiapX.Application.Gateways;
using FiapX.Application.Interfaces.DataSources;
using FiapX.Application.Interfaces.Services;
using FiapX.Application.Presenter.Users;
using FiapX.Application.UseCases.Users;
using FiapX.Application.UseCases.Users.Command;
using FiapX.Shared.DTO.User.Output;
using FiapX.Shared.DTO.User.Request;
using FiapX.Shared.Result;

namespace FiapX.Application.Controllers.Users;

public class UserController
{
    private readonly IUserDataSource _dataSource;
    private readonly IJwtService _jwtService;

    public UserController(IUserDataSource dataSource, IJwtService jwtService)
    {
        _dataSource = dataSource;
        _jwtService = jwtService;
    }

    public async Task<ICommandResult<UserOutputDto>> Register(UserRegisterRequestDto request)
    {
        var presenter = new UserPresenter("Usuário cadastrado com sucesso!");

        try
        {
            var command = new RegisterUserCommand(request.Name, request.Email, request.Password);

            var userGateway = UserGateway.Create(_dataSource);
            var useCase = RegisterUserUseCase.Create(userGateway);
            var user = await useCase.Run(command);

            return presenter.TransformObject(user);
        }
        catch (Exception ex)
        {
            return presenter.Error<UserOutputDto>(ex.Message);
        }
    }

    public async Task<ICommandResult<LoginOutputDto>> Login(UserLoginRequestDto request)
    {
        var presenter = new UserPresenter("Login realizado com sucesso!");

        try
        {
            var command = new LoginUserCommand(request.Email, request.Password);

            var userGateway = UserGateway.Create(_dataSource);
            var useCase = LoginUserUseCase.Create(userGateway, _jwtService);
            var (user, token, expiresAt) = await useCase.Run(command);

            return presenter.TransformLogin(user, token, expiresAt);
        }
        catch (Exception ex)
        {
            return presenter.Error<LoginOutputDto>(ex.Message);
        }
    }
}
