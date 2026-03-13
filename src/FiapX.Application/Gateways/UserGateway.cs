using FiapX.Application.Interfaces.DataSources;
using FiapX.Domain.Entities;
using FiapX.Shared.DTO.User.Input;

namespace FiapX.Application.Gateways;

public class UserGateway
{
    private readonly IUserDataSource _dataSource;

    private UserGateway(IUserDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public static UserGateway Create(IUserDataSource dataSource)
    {
        return new UserGateway(dataSource);
    }

    public async Task<User?> GetById(Guid id)
    {
        var user = await _dataSource.GetById(id);
        return user is not null ? MapToEntity(user) : null;
    }

    public async Task<User?> GetByEmail(string email)
    {
        var user = await _dataSource.GetByEmail(email);
        return user is not null ? MapToEntity(user) : null;
    }

    public async Task<bool> ExistsByEmail(string email)
    {
        return await _dataSource.ExistsByEmail(email);
    }

    public async Task CreateUser(User user)
    {
        var userInput = MapToDto(user);
        await _dataSource.Create(userInput);
    }

    private static User MapToEntity(UserInputDto dto)
    {
        return new User(dto.Id, dto.Name, dto.Email, dto.PasswordHash, dto.CreatedAt);
    }

    private static UserInputDto MapToDto(User user)
    {
        return new UserInputDto(user.Id, user.Name, user.Email, user.PasswordHash, user.CreatedAt);
    }
}
