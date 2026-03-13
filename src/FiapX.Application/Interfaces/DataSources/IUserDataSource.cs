using FiapX.Shared.DTO.User.Input;

namespace FiapX.Application.Interfaces.DataSources;

public interface IUserDataSource
{
    Task Create(UserInputDto user);
    Task<UserInputDto?> GetById(Guid id);
    Task<UserInputDto?> GetByEmail(string email);
    Task<bool> ExistsByEmail(string email);
}
