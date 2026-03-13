using FiapX.Infrastructure.DbContexts;
using FiapX.Infrastructure.DbModels;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace FiapX.Infrastructure.Persistence;

[ExcludeFromCodeCoverage]
public class DataSeeder
{
    private readonly AppDbContext _context;
    private bool _seedInDb = false;

    public DataSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task Initialize()
    {
        await SeedUsers();

        if (_seedInDb)
            await _context.SaveChangesAsync();
    }

    private async Task SeedUsers()
    {
        var userExists = await _context.Users.AnyAsync();

        if (!userExists)
        {
            var adminUser = new UserDbModel(
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
                "Admin",
                "admin@fiapx.com",
                HashPassword("admin123"),
                DateTime.UtcNow
            );

            await _context.Users.AddAsync(adminUser);
            _seedInDb = true;
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
