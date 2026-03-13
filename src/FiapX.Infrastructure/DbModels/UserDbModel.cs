using System.Diagnostics.CodeAnalysis;

namespace FiapX.Infrastructure.DbModels;

[ExcludeFromCodeCoverage]
public class UserDbModel
{
    public UserDbModel(Guid id, string name, string email, string passwordHash, DateTime createdAt)
    {
        Id = id;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
    }

    public UserDbModel() { }

    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<VideoDbModel> Videos { get; set; } = new List<VideoDbModel>();
}
