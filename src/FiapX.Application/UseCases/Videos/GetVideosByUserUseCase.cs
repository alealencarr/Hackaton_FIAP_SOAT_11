using FiapX.Application.Gateways;
using FiapX.Domain.Entities;

namespace FiapX.Application.UseCases.Videos;

public class GetVideosByUserUseCase
{
    private readonly VideoGateway _gateway;

    private GetVideosByUserUseCase(VideoGateway gateway)
    {
        _gateway = gateway;
    }

    public static GetVideosByUserUseCase Create(VideoGateway gateway)
    {
        return new GetVideosByUserUseCase(gateway);
    }

    public async Task<List<Video>> Run(Guid userId)
    {
        try
        {
            return await _gateway.GetByUserId(userId);
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao buscar vídeos: {ex.Message}");
        }
    }
}
