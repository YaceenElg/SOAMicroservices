using JeuService.Models;
using System.Collections.Concurrent;

namespace JeuService.Services
{
    public class GameEngine : IGameEngine
    {
        private readonly ConcurrentDictionary<int, GameState> _activeGames = new();
        private int _nextGameId = 1;
        private readonly Random _random = new();

        public GameState InitializeGame(string gameType, int playerId)
        {
            var gameId = _nextGameId++;
            // Normalize gameType to lowercase for consistency
            var normalizedGameType = (gameType ?? "connectfour").ToLower().Trim();
            
            var gameState = new GameState
            {
                GameId = gameId,
                GameType = normalizedGameType, // Store normalized type
                Status = "Playing",
                Score = 0,
                Level = 1,
                LastUpdated = DateTime.Now
            };

            // Initialize game-specific data
            switch (normalizedGameType)
            {
                case "connectfour":
                    gameState.GameData = InitializeConnectFour();
                    break;
                case "higherlower":
                    gameState.GameData = InitializeHigherLower();
                    break;
                case "memorymatch":
                    gameState.GameData = InitializeMemoryMatch();
                    break;
                default:
                    gameState.GameData = InitializeConnectFour();
                    break;
            }

            _activeGames[gameId] = gameState;
            return gameState;
        }

        public GameState ProcessAction(int gameId, string action, int playerId)
        {
            Console.WriteLine($"[GameEngine] ProcessAction: gameId={gameId}, action='{action}', playerId={playerId}");
            
            if (!_activeGames.TryGetValue(gameId, out var gameState))
            {
                Console.WriteLine($"[GameEngine] ProcessAction ERROR: Game {gameId} not found in _activeGames");
                Console.WriteLine($"[GameEngine] ProcessAction: Active games count: {_activeGames.Count}");
                foreach (var kvp in _activeGames)
                {
                    Console.WriteLine($"[GameEngine] ProcessAction: Active game - GameId={kvp.Key}, GameType={kvp.Value.GameType}, Status={kvp.Value.Status}");
                }
                throw new InvalidOperationException($"Game {gameId} not found");
            }

            // Preserve the original GameId - it should never change
            var originalGameId = gameState.GameId;
            Console.WriteLine($"[GameEngine] ProcessAction: Found game - GameId={originalGameId}, GameType={gameState.GameType}, Status={gameState.Status}");

            if (gameState.Status == "GameOver" || gameState.Status == "Won")
            {
                Console.WriteLine($"[GameEngine] ProcessAction: Game already ended, returning current state");
                return gameState;
            }

            gameState.LastUpdated = DateTime.Now;

            GameState result;
            switch (gameState.GameType.ToLower())
            {
                case "connectfour":
                    result = ProcessConnectFourAction(gameState, action);
                    break;
                case "higherlower":
                    result = ProcessHigherLowerAction(gameState, action);
                    break;
                case "memorymatch":
                    result = ProcessMemoryMatchAction(gameState, action);
                    break;
                default:
                    result = ProcessConnectFourAction(gameState, action);
                    break;
            }
            
            // Ensure GameId is always preserved
            if (result.GameId != originalGameId)
            {
                Console.WriteLine($"[GameEngine] ProcessAction WARNING: GameId changed from {originalGameId} to {result.GameId}, fixing it");
                result.GameId = originalGameId;
            }
            
            Console.WriteLine($"[GameEngine] ProcessAction: Returning GameState - GameId={result.GameId}, GameType={result.GameType}, Status={result.Status}");
            return result;
        }

        public GameState GetGameState(int gameId, int playerId)
        {
            if (!_activeGames.TryGetValue(gameId, out var gameState))
            {
                // Return a default game state instead of throwing exception
                // This can happen if the game was ended or expired
                // IMPORTANT: Garder le gameId original pour que le frontend puisse arrêter le polling
                return new GameState
                {
                    GameId = gameId, // Garder le gameId original, pas 0
                    GameType = "unknown",
                    Status = "GameOver",
                    Score = 0,
                    Level = 1,
                    LastUpdated = DateTime.Now,
                    GameData = new Dictionary<string, object>()
                };
            }

            return gameState;
        }

        public bool EndGame(int gameId, int playerId)
        {
            if (_activeGames.TryRemove(gameId, out var gameState))
            {
                gameState.Status = "GameOver";
                return true;
            }
            return false;
        }

        // Connect Four (Puissance 4)
        private Dictionary<string, object> InitializeConnectFour()
        {
            return new Dictionary<string, object>
            {
                // 6 rows x 7 cols flattened (row-major)
                { "board", Enumerable.Repeat("", 6 * 7).ToList() },
                { "currentPlayer", "R" }, // R or Y
                { "moves", 0 },
                { "lastMoveCol", -1 }
            };
        }

        private GameState ProcessConnectFourAction(GameState gameState, string action)
        {
            var boardList = gameState.GameData["board"] as List<object>;
            var board = boardList?.Select(x => x?.ToString() ?? "").ToList() ?? Enumerable.Repeat("", 6 * 7).ToList();
            var currentPlayer = gameState.GameData["currentPlayer"]?.ToString() ?? "R";
            var moves = Convert.ToInt32(gameState.GameData["moves"] ?? 0);

            if (action.StartsWith("drop_"))
            {
                var col = int.Parse(action.Substring(5));
                if (col >= 0 && col < 7)
                {
                    var placedIndex = FindConnectFourDropIndex(board, col);
                    if (placedIndex >= 0)
                    {
                        board[placedIndex] = currentPlayer;
                        moves++;
                        gameState.GameData["moves"] = moves;
                        gameState.GameData["board"] = board;
                        gameState.GameData["lastMoveCol"] = col;

                        if (CheckConnectFourWin(board, currentPlayer))
                        {
                            gameState.Status = "Won";
                            gameState.Score += 100;
                        }
                        else if (moves >= 42)
                        {
                            gameState.Status = "GameOver";
                        }
                        else
                        {
                            currentPlayer = currentPlayer == "R" ? "Y" : "R";
                            gameState.GameData["currentPlayer"] = currentPlayer;
                        }
                    }
                }
            }
            else if (action == "reset")
            {
                gameState.GameData = InitializeConnectFour();
                gameState.Status = "Playing";
                gameState.Score = 0;
                gameState.Level = 1;
            }

            return gameState;
        }

        private int FindConnectFourDropIndex(List<string> board, int col)
        {
            for (var row = 5; row >= 0; row--)
            {
                var idx = row * 7 + col;
                if (idx >= 0 && idx < board.Count && string.IsNullOrEmpty(board[idx]))
                    return idx;
            }
            return -1;
        }

        private bool CheckConnectFourWin(List<string> board, string player)
        {
            for (var row = 0; row < 6; row++)
            {
                for (var col = 0; col < 7; col++)
                {
                    if (GetCell(board, row, col) != player) continue;
                    if (CountInDirection(board, row, col, 0, 1, player) >= 4) return true;   // →
                    if (CountInDirection(board, row, col, 1, 0, player) >= 4) return true;   // ↓
                    if (CountInDirection(board, row, col, 1, 1, player) >= 4) return true;   // ↘
                    if (CountInDirection(board, row, col, -1, 1, player) >= 4) return true;  // ↗
                }
            }
            return false;
        }

        private string GetCell(List<string> board, int row, int col)
        {
            if (row < 0 || row >= 6 || col < 0 || col >= 7) return "";
            return board[row * 7 + col] ?? "";
        }

        private int CountInDirection(List<string> board, int row, int col, int dRow, int dCol, string player)
        {
            var count = 0;
            var r = row;
            var c = col;
            while (r >= 0 && r < 6 && c >= 0 && c < 7 && GetCell(board, r, c) == player)
            {
                count++;
                r += dRow;
                c += dCol;
            }
            return count;
        }

        // Higher / Lower
        private Dictionary<string, object> InitializeHigherLower()
        {
            var current = _random.Next(1, 14); // 1..13
            var next = _random.Next(1, 14);
            return new Dictionary<string, object>
            {
                { "current", current },
                { "next", next },
                { "revealed", false },
                { "streak", 0 },
                { "round", 1 },
                { "message", "Make a guess: Higher or Lower?" }
            };
        }

        private GameState ProcessHigherLowerAction(GameState gameState, string action)
        {
            var current = Convert.ToInt32(gameState.GameData["current"] ?? 1);
            var next = Convert.ToInt32(gameState.GameData["next"] ?? 1);
            var revealed = Convert.ToBoolean(gameState.GameData["revealed"] ?? false);
            var streak = Convert.ToInt32(gameState.GameData["streak"] ?? 0);
            var round = Convert.ToInt32(gameState.GameData["round"] ?? 1);

            if (action == "higher" || action == "lower")
            {
                if (revealed)
                {
                    gameState.GameData["message"] = "Round already resolved. Click Next.";
                    return gameState;
                }

                gameState.GameData["revealed"] = true;

                var isHigher = next > current;
                var isLower = next < current;
                var isTie = next == current;

                var correct =
                    (action == "higher" && isHigher) ||
                    (action == "lower" && isLower);

                if (isTie)
                {
                    gameState.GameData["message"] = $"Tie! {next} equals {current}.";
                    gameState.Score += 2;
                }
                else if (correct)
                {
                    streak++;
                    gameState.GameData["streak"] = streak;
                    gameState.Score += 10;
                    gameState.GameData["message"] = $"Correct! Next was {next}.";
                }
                else
                {
                    gameState.GameData["message"] = $"Wrong! Next was {next}.";
                    gameState.Status = "GameOver";
                }
            }
            else if (action == "next")
            {
                if (!revealed)
                {
                    gameState.GameData["message"] = "Make a guess first.";
                    return gameState;
                }

                round++;
                gameState.GameData["round"] = round;
                gameState.GameData["current"] = next;
                gameState.GameData["next"] = _random.Next(1, 14);
                gameState.GameData["revealed"] = false;
                gameState.GameData["message"] = "Make a guess: Higher or Lower?";

                gameState.Level = Math.Max(1, (round - 1) / 5 + 1);
            }
            else if (action == "reset")
            {
                gameState.GameData = InitializeHigherLower();
                gameState.Status = "Playing";
                gameState.Score = 0;
                gameState.Level = 1;
            }

            return gameState;
        }

        // Memory Match (4x4)
        private Dictionary<string, object> InitializeMemoryMatch()
        {
            var values = new List<int>();
            for (var i = 1; i <= 8; i++)
            {
                values.Add(i);
                values.Add(i);
            }

            values = values.OrderBy(_ => _random.Next()).ToList();

            return new Dictionary<string, object>
            {
                { "cards", values },
                { "revealed", new List<int>() },
                { "matched", new List<int>() },
                { "moves", 0 },
                { "message", "Flip two cards." }
            };
        }

        private GameState ProcessMemoryMatchAction(GameState gameState, string action)
        {
            var cardsObj = gameState.GameData["cards"] as List<object>;
            var cards = cardsObj?.Select(x => Convert.ToInt32(x)).ToList() ?? new List<int>();

            var revealedObj = gameState.GameData["revealed"] as List<object>;
            var revealed = revealedObj?.Select(x => Convert.ToInt32(x)).ToList() ?? new List<int>();

            var matchedObj = gameState.GameData["matched"] as List<object>;
            var matched = matchedObj?.Select(x => Convert.ToInt32(x)).ToList() ?? new List<int>();

            var moves = Convert.ToInt32(gameState.GameData["moves"] ?? 0);

            if (action.StartsWith("flip_"))
            {
                if (revealed.Count >= 2)
                {
                    gameState.GameData["message"] = "Resolve the pair (Next).";
                    return gameState;
                }

                var idx = int.Parse(action.Substring(5));
                if (idx < 0 || idx >= cards.Count) return gameState;
                if (matched.Contains(idx) || revealed.Contains(idx)) return gameState;

                revealed.Add(idx);
                gameState.GameData["revealed"] = revealed;

                if (revealed.Count == 2)
                {
                    moves++;
                    gameState.GameData["moves"] = moves;

                    var a = revealed[0];
                    var b = revealed[1];
                    if (cards[a] == cards[b])
                    {
                        matched.Add(a);
                        matched.Add(b);
                        gameState.GameData["matched"] = matched;
                        gameState.GameData["message"] = "Match!";
                        gameState.Score += 15;
                        revealed.Clear();
                        gameState.GameData["revealed"] = revealed;

                        if (matched.Count >= cards.Count)
                        {
                            gameState.Status = "Won";
                            gameState.Score += 100;
                        }
                    }
                    else
                    {
                        gameState.GameData["message"] = "No match. Click Next to continue.";
                    }
                }
            }
            else if (action == "next")
            {
                revealed.Clear();
                gameState.GameData["revealed"] = revealed;
                gameState.GameData["message"] = "Flip two cards.";
            }
            else if (action == "reset")
            {
                gameState.GameData = InitializeMemoryMatch();
                gameState.Status = "Playing";
                gameState.Score = 0;
                gameState.Level = 1;
            }

            gameState.Level = Math.Max(1, matched.Count / 4 + 1);
            return gameState;
        }

    }
}
