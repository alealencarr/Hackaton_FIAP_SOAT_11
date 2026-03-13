using System.Diagnostics.CodeAnalysis;

namespace FiapX.Shared.DTO.User.Input;

[ExcludeFromCodeCoverage]
public record UserInputDto(Guid Id, string Name, string Email, string PasswordHash, DateTime CreatedAt);
