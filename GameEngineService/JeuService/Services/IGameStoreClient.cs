using JeuService.SOAP;

namespace JeuService.Services
{
    public interface IGameStoreClient
    {
        Task<GameInfo?> GetGameAsync(int gameId);
    }
}
