using FiapX.Domain.Entities;
using FiapX.Shared.DTO.User.Output;
using FiapX.Shared.Result;

namespace FiapX.Application.Presenter.Users;

public class UserPresenter
{
    private readonly string _message;

    public UserPresenter(string? message = null)
    {
        _message = message ?? string.Empty;
    }

    public ICommandResult<UserOutputDto> TransformObject(User user)
    {
        return CommandResult<UserOutputDto>.Success(Transform(user), _message);
    }

    public ICommandResult<LoginOutputDto> TransformLogin(User user, string token, DateTime expiresAt)
    {
        var loginOutput = new LoginOutputDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = Transform(user)
        };

        return CommandResult<LoginOutputDto>.Success(loginOutput, _message);
    }

    public UserOutputDto Transform(User user)
    {
        return new UserOutputDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }

    public ICommandResult<T> Error<T>(string message)
    {
        return CommandResult<T>.Fail(message);
    }
}
