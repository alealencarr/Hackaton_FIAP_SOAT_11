using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FiapX.Shared.DTO.User.Request;

[ExcludeFromCodeCoverage]
public record UserRegisterRequestDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(255)]
    public required string Name { get; set; }

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [MaxLength(255)]
    public required string Email { get; set; }

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
    public required string Password { get; set; }
}

[ExcludeFromCodeCoverage]
public record UserLoginRequestDto
{
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "A senha é obrigatória.")]
    public required string Password { get; set; }
}
