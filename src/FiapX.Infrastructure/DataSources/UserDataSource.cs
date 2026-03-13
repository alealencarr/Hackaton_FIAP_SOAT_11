using FiapX.Application.Interfaces.DataSources;
using FiapX.Infrastructure.DbContexts;
using FiapX.Infrastructure.DbModels;
using FiapX.Shared.DTO.User.Input;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace FiapX.Infrastructure.DataSources;

[ExcludeFromCodeCoverage]
public class UserDataSource : IUserDataSource
{
    private readonly AppDbContext _appDbContext;

    public UserDataSource(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task Create(UserInputDto user)
    {
        var userDbModel = new UserDbModel(
            user.Id,
            user.Name,
            user.Email,
            user.PasswordHash,
            user.CreatedAt
        );

        await _appDbContext.AddAsync(userDbModel);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task<UserInputDto?> GetById(Guid id)
    {
        var user = await _appDbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        return user is not null ? MapToDto(user) : null;
    }

    public async Task<UserInputDto?> GetByEmail(string email)
    {
        var user = await _appDbContext.Users
            .AsNoTracking()
            .Where(x => x.Email == email.ToLowerInvariant())
            .FirstOrDefaultAsync();

        return user is not null ? MapToDto(user) : null;
    }

    public async Task<bool> ExistsByEmail(string email)
    {
        return await _appDbContext.Users
            .AsNoTracking()
            .AnyAsync(x => x.Email == email.ToLowerInvariant());
    }

    private static UserInputDto MapToDto(UserDbModel user)
    {
        return new UserInputDto(user.Id, user.Name, user.Email, user.PasswordHash, user.CreatedAt);
    }
}
