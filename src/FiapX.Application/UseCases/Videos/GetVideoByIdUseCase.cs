using FiapX.Application.Gateways;
using FiapX.Domain.Entities;

namespace FiapX.Application.UseCases.Videos;

public class GetVideoByIdUseCase
{
    private readonly VideoGateway _gateway;

    private GetVideoByIdUseCase(VideoGateway gateway)
    {
        _gateway = gateway;
    }

    public static GetVideoByIdUseCase Create(VideoGateway gateway)
    {
        return new GetVideoByIdUseCase(gateway);
    }

    public async Task<Video?> Run(Guid id)
    {
        try
        {
            return await _gateway.GetById(id);
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao buscar vídeo: {ex.Message}");
        }
    }
}
