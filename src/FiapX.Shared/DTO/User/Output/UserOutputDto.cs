using System.Diagnostics.CodeAnalysis;

namespace FiapX.Shared.DTO.User.Output;

[ExcludeFromCodeCoverage]
public record UserOutputDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

[ExcludeFromCodeCoverage]
public record LoginOutputDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserOutputDto User { get; set; } = null!;
}
