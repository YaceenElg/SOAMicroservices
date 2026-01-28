using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text.Json;
using JeuService.Services;
using JeuService.Models;
using System.ServiceModel.Channels;

namespace JeuService.SOAP
{
    public class JeuServiceSOAP : IJeuServiceSOAP
    {
        private readonly Services.IJeuService _jeuService;

        public JeuServiceSOAP(Services.IJeuService jeuService)
        {
            _jeuService = jeuService;
            Console.WriteLine($"[JeuServiceSOAP] Constructor called - IJeuService is {(jeuService != null ? "not null" : "null")}");
        }

        public GameInfo? GetGameInfo(int gameId)
        {
            return _jeuService.GetGameInfoAsync(gameId).Result;
        }

        public RecordInjuryStatusResponse RecordInjuryStatus(int gameId, int playerId, string status)
        {
            try
            {
                var injuryStatus = _jeuService.RecordInjuryStatusAsync(gameId, playerId, status).Result;
                return new RecordInjuryStatusResponse
                {
                    Success = true,
                    Message = "Injury status recorded successfully",
                    InjuryStatusId = injuryStatus.Id
                };
            }
            catch (Exception ex)
            {
                return new RecordInjuryStatusResponse
                {
                    Success = false,
                    Message = ex.Message,
                    InjuryStatusId = 0
                };
            }
        }

        public GetInjuryStatusResponse GetInjuryStatus(int gameId, int playerId)
        {
            var injuryStatus = _jeuService.GetInjuryStatusAsync(gameId, playerId).Result;
            
            if (injuryStatus == null)
            {
                return new GetInjuryStatusResponse
                {
                    Id = 0,
                    GameId = gameId,
                    PlayerId = playerId,
                    Status = "Unknown",
                    RecordedAt = DateTime.MinValue
                };
            }

            return new GetInjuryStatusResponse
            {
                Id = injuryStatus.Id,
                GameId = injuryStatus.GameId,
                PlayerId = injuryStatus.PlayerId,
                Status = injuryStatus.Status,
                RecordedAt = injuryStatus.RecordedAt
            };
        }

        public DevelopedGameResponse AddDevelopedGame(DevelopedGameRequest game)
        {
            var domainGame = new Models.DevelopedGame
            {
                Title = game.Title,
                Description = game.Description,
                Developer = game.Developer,
                Version = game.Version,
                GameUrl = game.GameUrl,
                ReleaseDate = game.ReleaseDate,
                IsActive = game.IsActive
            };

            var result = _jeuService.AddDevelopedGameAsync(domainGame).Result;
            return new DevelopedGameResponse
            {
                Id = result.Id,
                Title = result.Title,
                Description = result.Description,
                Developer = result.Developer,
                Version = result.Version,
                GameUrl = result.GameUrl,
                ReleaseDate = result.ReleaseDate,
                IsActive = result.IsActive
            };
        }

        public DevelopedGameResponse? GetDevelopedGame(int id)
        {
            var game = _jeuService.GetDevelopedGameAsync(id).Result;
            if (game == null) return null;

            return new DevelopedGameResponse
            {
                Id = game.Id,
                Title = game.Title,
                Description = game.Description,
                Developer = game.Developer,
                Version = game.Version,
                GameUrl = game.GameUrl,
                ReleaseDate = game.ReleaseDate,
                IsActive = game.IsActive
            };
        }

        public GetAllDevelopedGamesResponse GetAllDevelopedGames()
        {
            var games = _jeuService.GetAllDevelopedGamesAsync().Result;
            return new GetAllDevelopedGamesResponse
            {
                Games = games.Select(g => new DevelopedGameResponse
                {
                    Id = g.Id,
                    Title = g.Title,
                    Description = g.Description,
                    Developer = g.Developer,
                    Version = g.Version,
                    GameUrl = g.GameUrl,
                    ReleaseDate = g.ReleaseDate,
                    IsActive = g.IsActive
                }).ToList()
            };
        }

        public DevelopedGameResponse? UpdateDevelopedGame(int id, DevelopedGameRequest game)
        {
            var domainGame = new Models.DevelopedGame
            {
                Title = game.Title,
                Description = game.Description,
                Developer = game.Developer,
                Version = game.Version,
                GameUrl = game.GameUrl,
                ReleaseDate = game.ReleaseDate,
                IsActive = game.IsActive
            };

            var result = _jeuService.UpdateDevelopedGameAsync(id, domainGame).Result;
            if (result == null) return null;

            return new DevelopedGameResponse
            {
                Id = result.Id,
                Title = result.Title,
                Description = result.Description,
                Developer = result.Developer,
                Version = result.Version,
                GameUrl = result.GameUrl,
                ReleaseDate = result.ReleaseDate,
                IsActive = result.IsActive
            };
        }

        public bool DeleteDevelopedGame(int id)
        {
            return _jeuService.DeleteDevelopedGameAsync(id).Result;
        }

        // Game Engine operations
        public GameStateResponse StartGame(string gameType, int playerId)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine($"[JeuServiceSOAP] StartGame METHOD ENTRY - gameType='{gameType}', playerId={playerId}");
            Console.WriteLine($"[JeuServiceSOAP] _jeuService is {( _jeuService != null ? "NOT NULL" : "NULL")}");
            
            try
            {
                Console.WriteLine($"[JeuServiceSOAP] Calling _jeuService.StartGameAsync...");
                var gameState = _jeuService.StartGameAsync(gameType, playerId).Result;
                
                if (gameState == null)
                {
                    Console.WriteLine($"[JeuServiceSOAP] ERROR: gameState is null!");
                    throw new InvalidOperationException("GameState is null after StartGameAsync");
                }
                
                Console.WriteLine($"[JeuServiceSOAP] GameState received - GameId={gameState.GameId}, Type={gameState.GameType}, Status={gameState.Status}, Score={gameState.Score}");
                
                // S'assurer que le GameType n'est jamais null ou vide
                if (string.IsNullOrEmpty(gameState.GameType))
                {
                    Console.WriteLine($"[JeuServiceSOAP] WARNING: GameType is null or empty, using requested gameType '{gameType}'");
                    gameState.GameType = gameType;
                }
                
                Console.WriteLine($"[JeuServiceSOAP] Mapping GameState to Response...");
                // For StartGame, use the GameState's GameId as fallback
                var response = MapGameStateToResponse(gameState, gameType, gameState?.GameId);
                Console.WriteLine($"[JeuServiceSOAP] Response created - GameId={response.GameId}, GameType={response.GameType}, GameDataJson length={response.GameDataJson?.Length ?? 0}");
                Console.WriteLine("==========================================");
                return response;
            }
            catch (AggregateException ae)
            {
                var innerEx = ae.InnerException ?? ae;
                Console.WriteLine($"[JeuServiceSOAP] StartGame AggregateException: {innerEx.Message}");
                Console.WriteLine($"[JeuServiceSOAP] StackTrace: {innerEx.StackTrace}");
                Console.WriteLine("==========================================");
                throw new FaultException<string>(innerEx.Message, new FaultReason(innerEx.Message));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JeuServiceSOAP] StartGame Exception: {ex.Message}");
                Console.WriteLine($"[JeuServiceSOAP] Exception Type: {ex.GetType().Name}");
                Console.WriteLine($"[JeuServiceSOAP] StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[JeuServiceSOAP] InnerException: {ex.InnerException.Message}");
                }
                Console.WriteLine("==========================================");
                throw new FaultException<string>($"Error starting game: {ex.Message}", new FaultReason(ex.Message));
            }
        }

        public GameStateResponse PlayAction(int gameId, string action, int playerId)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine($"[JeuServiceSOAP] PlayAction METHOD ENTRY - gameId={gameId}, action='{action}', playerId={playerId}");
            
            try
            {
                if (gameId <= 0)
                {
                    Console.WriteLine($"[JeuServiceSOAP] PlayAction ERROR: Invalid gameId={gameId}");
                    throw new ArgumentException($"Invalid game ID: {gameId}. Game ID must be greater than 0.");
                }
                
                Console.WriteLine($"[JeuServiceSOAP] Calling _jeuService.PlayActionAsync...");
                var gameState = _jeuService.PlayActionAsync(gameId, action, playerId).Result;
                
                if (gameState == null)
                {
                    Console.WriteLine($"[JeuServiceSOAP] PlayAction ERROR: gameState is null!");
                    throw new InvalidOperationException("GameState is null after PlayActionAsync");
                }
                
                Console.WriteLine($"[JeuServiceSOAP] PlayAction: GameState received - GameId={gameState.GameId}, Type={gameState.GameType}, Status={gameState.Status}, Score={gameState.Score}");
                
                // Ensure GameId is preserved - use requested gameId as fallback
                if (gameState.GameId <= 0)
                {
                    Console.WriteLine($"[JeuServiceSOAP] PlayAction WARNING: GameState has invalid GameId={gameState.GameId}, using requested gameId={gameId}");
                    gameState.GameId = gameId; // Preserve the requested gameId
                }
                
                Console.WriteLine($"[JeuServiceSOAP] Mapping GameState to Response with fallback gameId={gameId}...");
                var response = MapGameStateToResponse(gameState, gameState?.GameType, gameId);
                Console.WriteLine($"[JeuServiceSOAP] PlayAction: Response created - GameId={response.GameId}, GameType={response.GameType}, GameDataJson length={response.GameDataJson?.Length ?? 0}");
                Console.WriteLine("==========================================");
                return response;
            }
            catch (AggregateException ae)
            {
                var innerEx = ae.InnerException ?? ae;
                Console.WriteLine($"[JeuServiceSOAP] PlayAction AggregateException: {innerEx.Message}");
                Console.WriteLine($"[JeuServiceSOAP] StackTrace: {innerEx.StackTrace}");
                Console.WriteLine("==========================================");
                throw new FaultException<string>(innerEx.Message, new FaultReason(innerEx.Message));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JeuServiceSOAP] PlayAction Exception: {ex.Message}");
                Console.WriteLine($"[JeuServiceSOAP] Exception Type: {ex.GetType().Name}");
                Console.WriteLine($"[JeuServiceSOAP] StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[JeuServiceSOAP] InnerException: {ex.InnerException.Message}");
                }
                Console.WriteLine("==========================================");
                throw new FaultException<string>($"Error playing action: {ex.Message}", new FaultReason(ex.Message));
            }
        }

        public GameStateResponse GetGameState(int gameId, int playerId)
        {
            Console.WriteLine($"[JeuServiceSOAP] GetGameState: gameId={gameId}, playerId={playerId}");
            var gameState = _jeuService.GetGameStateAsync(gameId, playerId).Result;
            
            // Ensure GameId is preserved
            if (gameState != null && gameState.GameId <= 0)
            {
                Console.WriteLine($"[JeuServiceSOAP] GetGameState WARNING: GameState has invalid GameId={gameState.GameId}, using requested gameId={gameId}");
                gameState.GameId = gameId;
            }
            
            return MapGameStateToResponse(gameState, gameState?.GameType, gameId);
        }

        public bool EndGame(int gameId, int playerId)
        {
            return _jeuService.EndGameAsync(gameId, playerId).Result;
        }

        private GameStateResponse MapGameStateToResponse(GameState gameState, string? fallbackGameType = null, int? fallbackGameId = null)
        {
            try
            {
                if (gameState == null)
                {
                    Console.WriteLine($"[JeuServiceSOAP] MapGameStateToResponse ERROR: gameState is null!");
                    throw new ArgumentNullException(nameof(gameState));
                }
                
                var gameDataJson = JsonSerializer.Serialize(gameState.GameData ?? new Dictionary<string, object>());
                
                // S'assurer que le GameType n'est jamais null ou vide
                var gameType = gameState.GameType;
                if (string.IsNullOrEmpty(gameType))
                {
                    gameType = fallbackGameType ?? "tictactoe";
                    Console.WriteLine($"[JeuServiceSOAP] MapGameStateToResponse: GameType was empty, using fallback '{gameType}'");
                }
                
                Console.WriteLine($"[JeuServiceSOAP] MapGameStateToResponse: GameId={gameState.GameId}, GameType='{gameType}', Status='{gameState.Status}'");
                Console.WriteLine($"[JeuServiceSOAP] MapGameStateToResponse: Serialized GameDataJson length={gameDataJson.Length}");
                
                // Ensure GameId is never 0 or negative - use fallback if provided
                var responseGameId = gameState.GameId;
                if (responseGameId <= 0)
                {
                    if (fallbackGameId.HasValue && fallbackGameId.Value > 0)
                    {
                        Console.WriteLine($"[JeuServiceSOAP] MapGameStateToResponse WARNING: GameState has invalid GameId={responseGameId}, using fallback gameId={fallbackGameId.Value}");
                        responseGameId = fallbackGameId.Value;
                    }
                    else
                    {
                        Console.WriteLine($"[JeuServiceSOAP] MapGameStateToResponse ERROR: GameState has invalid GameId={responseGameId} and no fallback provided");
                        throw new InvalidOperationException($"Invalid GameId in GameState: {responseGameId}");
                    }
                }
                
                var response = new GameStateResponse
                {
                    GameId = responseGameId, // Use validated gameId
                    GameType = gameType, // Always defined now
                    Status = gameState.Status ?? "Playing",
                    Score = gameState.Score,
                    Level = gameState.Level,
                    GameDataJson = gameDataJson,
                    LastUpdated = gameState.LastUpdated
                };
                
                Console.WriteLine($"[JeuServiceSOAP] MapGameStateToResponse: Response created - GameId={response.GameId}, GameType='{response.GameType}', Status='{response.Status}'");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JeuServiceSOAP] MapGameStateToResponse Error: {ex.Message}");
                Console.WriteLine($"[JeuServiceSOAP] MapGameStateToResponse StackTrace: {ex.StackTrace}");
                // Use fallback gameId if available, otherwise 0 (but log warning)
                var errorGameId = fallbackGameId ?? gameState?.GameId ?? 0;
                if (errorGameId <= 0)
                {
                    Console.WriteLine($"[JeuServiceSOAP] MapGameStateToResponse ERROR: Cannot create response - all gameIds are invalid");
                    errorGameId = 0; // Last resort
                }
                
                return new GameStateResponse
                {
                    GameId = errorGameId,
                    GameType = gameState?.GameType ?? fallbackGameType ?? "tictactoe",
                    Status = gameState?.Status ?? "Error",
                    Score = gameState?.Score ?? 0,
                    Level = gameState?.Level ?? 1,
                    GameDataJson = "{}",
                    LastUpdated = gameState?.LastUpdated ?? DateTime.Now
                };
            }
        }
    }
}
