using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace FiapX.Shared.DTO.Video.Request;

[ExcludeFromCodeCoverage]
public record VideoUploadRequestDto
{
    [Required(ErrorMessage = "O arquivo de vídeo é obrigatório.")]
    public required IFormFile VideoFile { get; set; }
}
