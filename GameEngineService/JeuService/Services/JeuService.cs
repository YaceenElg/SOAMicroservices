using JeuService.Models;
using JeuService.SOAP;

namespace JeuService.Services
{
    public class JeuService : IJeuService
    {
        private readonly IGameStoreClient _gameStoreClient;
        private readonly IGameEngine _gameEngine;
        private readonly List<InjuryStatus> _injuryStatuses = new();
        private readonly List<DevelopedGame> _developedGames = new();
        private int _nextInjuryId = 1;
        private int _nextGameId = 1;

        public JeuService(IGameStoreClient gameStoreClient, IGameEngine gameEngine)
        {
            _gameStoreClient = gameStoreClient;
            _gameEngine = gameEngine;
            InitializeDevelopedGames();
        }

        private void InitializeDevelopedGames()
        {
            // Jeux du GameEngine disponibles - UNIQUEMENT les jeux supportés
            _developedGames.AddRange(new List<DevelopedGame>
            {
                new DevelopedGame
                {
                    Id = _nextGameId++,
                    Title = "Connect Four",
                    Description = "Drop discs and connect four in a row. Two-player local mode.",
                    Developer = "JeuService Team",
                    Version = "1.0.0",
                    GameUrl = "/games/connectfour",
                    ReleaseDate = DateTime.Now.AddMonths(-1),
                    IsActive = true
                },
                new DevelopedGame
                {
                    Id = _nextGameId++,
                    Title = "Higher / Lower",
                    Description = "Guess if the next number will be higher or lower. Build a streak!",
                    Developer = "JeuService Team",
                    Version = "1.0.0",
                    GameUrl = "/games/higherlower",
                    ReleaseDate = DateTime.Now.AddMonths(-1),
                    IsActive = true
                },
                new DevelopedGame
                {
                    Id = _nextGameId++,
                    Title = "Memory Match",
                    Description = "Flip cards to find all matching pairs. Finish with fewer moves!",
                    Developer = "JeuService Team",
                    Version = "1.0.0",
                    GameUrl = "/games/memorymatch",
                    ReleaseDate = DateTime.Now.AddMonths(-1),
                    IsActive = true
                }
            });
        }

        public async Task<GameInfo?> GetGameInfoAsync(int gameId)
        {
            return await _gameStoreClient.GetGameAsync(gameId);
        }

        public async Task<InjuryStatus> RecordInjuryStatusAsync(int gameId, int playerId, string status)
        {
            // Validate status
            if (status != "Alive" && status != "Injured" && status != "Dead")
            {
                throw new ArgumentException("Status must be 'Alive', 'Injured', or 'Dead'");
            }

            // Check if status already exists for this game/player
            var existing = _injuryStatuses.FirstOrDefault(s => s.GameId == gameId && s.PlayerId == playerId);
            
            if (existing != null)
            {
                existing.Status = status;
                existing.RecordedAt = DateTime.Now;
                return existing;
            }

            var injuryStatus = new InjuryStatus
            {
                Id = _nextInjuryId++,
                GameId = gameId,
                PlayerId = playerId,
                Status = status,
                RecordedAt = DateTime.Now
            };

            _injuryStatuses.Add(injuryStatus);
            return injuryStatus;
        }

        public async Task<InjuryStatus?> GetInjuryStatusAsync(int gameId, int playerId)
        {
            return _injuryStatuses.FirstOrDefault(s => s.GameId == gameId && s.PlayerId == playerId);
        }

        public async Task<DevelopedGame> AddDevelopedGameAsync(DevelopedGame game)
        {
            game.Id = _nextGameId++;
            _developedGames.Add(game);
            return game;
        }

        public async Task<DevelopedGame?> GetDevelopedGameAsync(int id)
        {
            return _developedGames.FirstOrDefault(g => g.Id == id);
        }

        public async Task<List<DevelopedGame>> GetAllDevelopedGamesAsync()
        {
            return _developedGames.Where(g => g.IsActive).ToList();
        }

        public async Task<DevelopedGame?> UpdateDevelopedGameAsync(int id, DevelopedGame game)
        {
            var existing = _developedGames.FirstOrDefault(g => g.Id == id);
            if (existing == null)
                return null;

            existing.Title = game.Title;
            existing.Description = game.Description;
            existing.Developer = game.Developer;
            existing.Version = game.Version;
            existing.GameUrl = game.GameUrl;
            existing.ReleaseDate = game.ReleaseDate;
            existing.IsActive = game.IsActive;

            return existing;
        }

        public async Task<bool> DeleteDevelopedGameAsync(int id)
        {
            var game = _developedGames.FirstOrDefault(g => g.Id == id);
            if (game == null)
                return false;

            game.IsActive = false; // Soft delete
            return true;
        }

        // Game Engine operations
        public async Task<GameState> StartGameAsync(string gameType, int playerId)
        {
            return _gameEngine.InitializeGame(gameType, playerId);
        }

        public async Task<GameState> PlayActionAsync(int gameId, string action, int playerId)
        {
            return _gameEngine.ProcessAction(gameId, action, playerId);
        }

        public async Task<GameState> GetGameStateAsync(int gameId, int playerId)
        {
            return _gameEngine.GetGameState(gameId, playerId);
        }

        public async Task<bool> EndGameAsync(int gameId, int playerId)
        {
            return _gameEngine.EndGame(gameId, playerId);
        }
    }
}
