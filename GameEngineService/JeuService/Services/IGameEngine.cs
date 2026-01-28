using JeuService.Models;

namespace JeuService.Services
{
    public interface IGameEngine
    {
        GameState InitializeGame(string gameType, int playerId);
        GameState ProcessAction(int gameId, string action, int playerId);
        GameState GetGameState(int gameId, int playerId);
        bool EndGame(int gameId, int playerId);
    }
}
