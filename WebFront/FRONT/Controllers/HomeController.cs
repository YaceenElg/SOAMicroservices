using System.Diagnostics;
using FRONT.Models;
using FRONT.Services;
using Microsoft.AspNetCore.Mvc;

namespace FRONT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserServiceClient _userServiceClient;
        private readonly GameStoreServiceClient _gameStoreServiceClient;

        public HomeController(ILogger<HomeController> logger, UserServiceClient userServiceClient, GameStoreServiceClient gameStoreServiceClient)
        {
            _logger = logger;
            _userServiceClient = userServiceClient;
            _gameStoreServiceClient = gameStoreServiceClient;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            return RedirectToAction("Categories");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Console.WriteLine($"[HomeController] Login attempt for username: '{model.Username}'");
                    var authenticated = await _userServiceClient.AuthenticateAsync(model.Username, model.Password);
                    Console.WriteLine($"[HomeController] Login: Authenticated = {authenticated}");
                    System.Diagnostics.Debug.WriteLine($"Login: Authenticated = {authenticated}");
                    
                    if (authenticated == true)
                    {
                        Console.WriteLine($"[HomeController] Authentication successful, fetching user details...");
                        var user = await _userServiceClient.GetUserByUsernameAsync(model.Username);
                        Console.WriteLine($"[HomeController] Login: User = {(user != null ? $"Id={user.Id}, Username={user.Username}, Email={user.Email}" : "null")}");
                        System.Diagnostics.Debug.WriteLine($"Login: User = {(user != null ? $"Id={user.Id}, Username={user.Username}" : "null")}");
                        
                        if (user != null && !string.IsNullOrEmpty(user.Username))
                        {
                            HttpContext.Session.SetString("UserId", user.Id.ToString());
                            HttpContext.Session.SetString("Username", user.Username ?? string.Empty);
                            Console.WriteLine($"[HomeController] Login: Session set for UserId={user.Id}, Username={user.Username}, redirecting to Categories");
                            System.Diagnostics.Debug.WriteLine($"Login: Session set, redirecting to Categories");
                            return RedirectToAction("Categories");
                        }
                        else
                        {
                            Console.WriteLine($"[HomeController] Login: User is null or Username is empty");
                            System.Diagnostics.Debug.WriteLine($"Login: User is null or Username is empty");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[HomeController] Login: Authentication returned false");
                        System.Diagnostics.Debug.WriteLine($"Login: Authentication returned false");
                    }
                    ModelState.AddModelError("", "Invalid username or password");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[HomeController] Login Exception: {ex.Message}");
                    Console.WriteLine($"[HomeController] Login Exception StackTrace: {ex.StackTrace}");
                    ModelState.AddModelError("", $"Login error: {ex.Message}. Please ensure UserService is running.");
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("", "Passwords do not match");
                    return View(model);
                }

                try
                {
                    var user = await _userServiceClient.RegisterAsync(model.Username, model.Email, model.Password);
                    if (user != null && !string.IsNullOrEmpty(user.Username))
                    {
                        HttpContext.Session.SetString("UserId", user.Id.ToString());
                        HttpContext.Session.SetString("Username", user.Username ?? string.Empty);
                        return RedirectToAction("Categories");
                    }
                    ModelState.AddModelError("", "Registration failed. Please try again.");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Registration failed: {ex.Message}");
                }
            }
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Categories()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var categories = await _gameStoreServiceClient.GetAllCategoriesAsync();
            return View(categories);
        }

        public async Task<IActionResult> Games(int categoryId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var games = await _gameStoreServiceClient.GetGamesByCategoryAsync(categoryId);
            ViewBag.CategoryId = categoryId;
            return View(games);
        }

        public async Task<IActionResult> GameDetails(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var game = await _gameStoreServiceClient.GetGameAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        public async Task<IActionResult> LaunchGame(int id, string gameUrl)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var jeuServiceClient = HttpContext.RequestServices.GetRequiredService<JeuServiceClient>();
            
            // Determine game type from gameUrl - mapping basé sur les jeux du GameEngine
            // IMPORTANT: Seuls les jeux du GameEngine sont supportés
            string gameType = "connectfour"; // Default
            
            if (string.IsNullOrEmpty(gameUrl))
            {
                gameType = "connectfour";
            }
            else
            {
                // Normaliser l'URL pour la comparaison
                var normalizedUrl = gameUrl.ToLower().Trim();
                
                if (normalizedUrl.Contains("connectfour") || normalizedUrl.Contains("connect-four") || normalizedUrl == "/games/connectfour")
                {
                    gameType = "connectfour";
                }
                else if (normalizedUrl.Contains("higherlower") || normalizedUrl.Contains("higher-lower") || normalizedUrl == "/games/higherlower")
                {
                    gameType = "higherlower";
                }
                else if (normalizedUrl.Contains("memorymatch") || normalizedUrl.Contains("memory-match") || normalizedUrl == "/games/memorymatch")
                {
                    gameType = "memorymatch";
                }
                else
                {
                    // Fallback: Extract game type directly from URL path
                    var urlParts = gameUrl.Trim('/').Split('/');
                    if (urlParts.Length > 0)
                    {
                        var gameName = urlParts[urlParts.Length - 1].ToLower().Trim();
                        if (gameName == "connectfour" || gameName == "connect-four")
                            gameType = "connectfour";
                        else if (gameName == "higherlower" || gameName == "higher-lower")
                            gameType = "higherlower";
                        else if (gameName == "memorymatch" || gameName == "memory-match")
                            gameType = "memorymatch";
                        else
                        {
                            gameType = gameName; // Use gameName directly if not recognized
                            Console.WriteLine($"[HomeController] LaunchGame: Unknown game name '{gameName}' from URL '{gameUrl}', using as gameType");
                        }
                    }
                }
                
                Console.WriteLine($"[HomeController] LaunchGame: Extracted gameType='{gameType}' from gameUrl='{gameUrl}'");
            }
            
            Console.WriteLine($"[HomeController] LaunchGame: Mapped gameUrl='{gameUrl}' to gameType='{gameType}'");

            try
            {
                Console.WriteLine($"[HomeController] LaunchGame: Starting game type='{gameType}', userId={userId}, gameUrl='{gameUrl}'");
                var gameState = await jeuServiceClient.StartGameAsync(gameType, int.Parse(userId));
                Console.WriteLine($"[HomeController] LaunchGame: GameState received - GameId={gameState?.GameId}, GameType={gameState?.GameType ?? "NULL"}, Status={gameState?.Status}, Score={gameState?.Score}");
                Console.WriteLine($"[HomeController] LaunchGame: GameState full object - GameId={gameState?.GameId}, GameType='{gameState?.GameType}', Status='{gameState?.Status}', Score={gameState?.Score}, Level={gameState?.Level}");
                Console.WriteLine($"[HomeController] LaunchGame: GameDataJson length={gameState?.GameDataJson?.Length ?? 0}");
                
                if (gameState == null)
                {
                    Console.WriteLine($"[HomeController] LaunchGame ERROR: gameState is null!");
                    ViewBag.Error = "Failed to start game. GameState is null.";
                    ViewBag.GameId = id;
                    ViewBag.GameUrl = gameUrl;
                    ViewBag.RequestedGameType = gameType;
                    return View();
                }
                
                // S'assurer que le GameType n'est jamais null ou vide
                if (string.IsNullOrEmpty(gameState.GameType))
                {
                    Console.WriteLine($"[HomeController] LaunchGame ERROR: GameType is null or empty!");
                    Console.WriteLine($"[HomeController] LaunchGame: Setting GameType to requested type '{gameType}'");
                    gameState.GameType = gameType; // Fallback to requested type
                }
                
                ViewBag.GameState = gameState;
                ViewBag.GameId = id;
                ViewBag.GameUrl = gameUrl;
                ViewBag.RequestedGameType = gameType; // Passer le gameType au ViewBag pour la vue
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HomeController] LaunchGame Error: {ex.Message}");
                Console.WriteLine($"[HomeController] LaunchGame StackTrace: {ex.StackTrace}");
                ViewBag.Error = $"Error starting game: {ex.Message}";
                ViewBag.GameId = id;
                ViewBag.GameUrl = gameUrl;
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> PlayGameAction([FromBody] PlayGameActionRequest request)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { error = "Not authenticated" });
            }

            // Validate request
            if (request == null)
            {
                Console.WriteLine($"[HomeController] PlayGameAction ERROR: request is null");
                return Json(new { error = "Invalid request" });
            }

            if (request.GameId <= 0)
            {
                Console.WriteLine($"[HomeController] PlayGameAction ERROR: Invalid gameId={request.GameId}, action='{request.Action}', playerId={request.PlayerId}");
                return Json(new { error = $"Invalid game ID: {request.GameId}. Game ID must be greater than 0." });
            }

            if (string.IsNullOrEmpty(request.Action))
            {
                Console.WriteLine($"[HomeController] PlayGameAction ERROR: Action is null or empty");
                return Json(new { error = "Action is required" });
            }

            try
            {
                Console.WriteLine($"[HomeController] PlayGameAction: gameId={request.GameId}, action='{request.Action}', playerId={request.PlayerId}");
                var jeuServiceClient = HttpContext.RequestServices.GetRequiredService<JeuServiceClient>();
                var gameState = await jeuServiceClient.PlayActionAsync(request.GameId, request.Action, request.PlayerId);
                
                if (gameState == null)
                {
                    Console.WriteLine($"[HomeController] PlayGameAction ERROR: gameState is null");
                    return Json(new { error = "Game state is null" });
                }

                // Ensure GameId is always valid - use requested gameId as fallback
                var responseGameId = gameState.GameId;
                if (responseGameId <= 0)
                {
                    Console.WriteLine($"[HomeController] PlayGameAction WARNING: Response has invalid GameId={responseGameId}, using requested gameId={request.GameId}");
                    responseGameId = request.GameId;
                }

                Console.WriteLine($"[HomeController] PlayGameAction: Success - GameId={responseGameId}, GameType='{gameState.GameType}', Status='{gameState.Status}'");
                
                return Json(new
                {
                    GameId = responseGameId, // Use validated gameId
                    GameType = gameState.GameType ?? "connectfour",
                    Status = gameState.Status ?? "Playing",
                    Score = gameState.Score,
                    Level = gameState.Level,
                    GameDataJson = gameState.GameDataJson ?? "{}"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HomeController] PlayGameAction Exception: {ex.Message}");
                Console.WriteLine($"[HomeController] PlayGameAction StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[HomeController] PlayGameAction InnerException: {ex.InnerException.Message}");
                }
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetGameState(int gameId, int playerId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { error = "Not authenticated" });
            }

            try
            {
                // Vérifier que le gameId est valide
                if (gameId <= 0)
                {
                    Console.WriteLine($"[HomeController] GetGameState ERROR: Invalid gameId={gameId}");
                    return Json(new { error = "Invalid game ID", GameId = gameId, Status = "Error" });
                }

                var jeuServiceClient = HttpContext.RequestServices.GetRequiredService<JeuServiceClient>();
                var gameState = await jeuServiceClient.GetGameStateAsync(gameId, playerId);
                
                // Vérifier que le gameState est valide
                if (gameState == null)
                {
                    Console.WriteLine($"[HomeController] GetGameState ERROR: gameState is null for gameId={gameId}");
                    return Json(new { error = "Game state not found", GameId = gameId, Status = "Error" });
                }

                // Si le jeu n'existe plus (GameType="unknown"), arrêter le polling en retournant un état d'erreur
                if (gameState.GameType == "unknown" || (gameState.GameId == 0 && gameId > 0))
                {
                    Console.WriteLine($"[HomeController] GetGameState: Game {gameId} is over or unknown, GameId={gameState.GameId}");
                    return Json(new
                    {
                        GameId = gameId, // Garder le gameId original
                        GameType = "unknown",
                        Status = "GameOver",
                        Score = 0,
                        Level = 1,
                        GameDataJson = "{}",
                        error = "Game session expired"
                    });
                }

                // Ensure GameType is never null or empty
                var gameType = gameState.GameType;
                if (string.IsNullOrWhiteSpace(gameType))
                {
                    Console.WriteLine($"[HomeController] GetGameState WARNING: GameType is null/empty for GameId={gameId}, using fallback");
                    gameType = "connectfour"; // Fallback
                }
                
                Console.WriteLine($"[HomeController] GetGameState: Returning - GameId={gameState.GameId}, GameType='{gameType}', Status='{gameState.Status}'");
                
                return Json(new
                {
                    GameId = gameState.GameId,
                    GameType = gameType,
                    Status = gameState.Status ?? "Playing",
                    Score = gameState.Score,
                    Level = gameState.Level,
                    GameDataJson = gameState.GameDataJson ?? "{}"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HomeController] GetGameState Exception: {ex.Message}");
                Console.WriteLine($"[HomeController] GetGameState StackTrace: {ex.StackTrace}");
                return Json(new { error = ex.Message, GameId = gameId, Status = "Error" });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class PlayGameActionRequest
    {
        public int GameId { get; set; }
        public string Action { get; set; } = string.Empty;
        public int PlayerId { get; set; }
    }
}
