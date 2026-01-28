using JeuService.Models;
using JeuService.SOAP;

namespace JeuService.Services
{
    public interface IJeuService
    {
        // GameStoreService consumption
        Task<GameInfo?> GetGameInfoAsync(int gameId);
        
        // Injury status operations
        Task<InjuryStatus> RecordInjuryStatusAsync(int gameId, int playerId, string status);
        Task<InjuryStatus?> GetInjuryStatusAsync(int gameId, int playerId);
        
        // Developed games operations
        Task<DevelopedGame> AddDevelopedGameAsync(DevelopedGame game);
        Task<DevelopedGame?> GetDevelopedGameAsync(int id);
        Task<List<DevelopedGame>> GetAllDevelopedGamesAsync();
        Task<DevelopedGame?> UpdateDevelopedGameAsync(int id, DevelopedGame game);
        Task<bool> DeleteDevelopedGameAsync(int id);
        
        // Game Engine operations
        Task<GameState> StartGameAsync(string gameType, int playerId);
        Task<GameState> PlayActionAsync(int gameId, string action, int playerId);
        Task<GameState> GetGameStateAsync(int gameId, int playerId);
        Task<bool> EndGameAsync(int gameId, int playerId);
    }
}
