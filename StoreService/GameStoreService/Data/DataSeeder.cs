using GameStoreService.Models;

namespace GameStoreService.Data
{
    public static class DataSeeder
    {
        /// <summary>
        /// Liste des jeux disponibles dans JeuService/GameEngine
        /// Cette liste doit être synchronisée avec les jeux disponibles dans JeuService
        /// </summary>
        private static readonly Dictionary<string, (string Title, string Description, string CategoryName, decimal Price)> JeuServiceGames = new()
        {
            { "connectfour", ("Connect Four", "Drop discs and connect four in a row. Two-player local mode.", "Strategy", 0.00m) },
            { "higherlower", ("Higher / Lower", "Guess if the next number will be higher or lower. Build a streak!", "Action", 0.00m) },
            { "memorymatch", ("Memory Match", "Flip cards to find all matching pairs. Finish with fewer moves!", "Puzzle", 0.00m) }
        };

        public static void SeedGames(GameStoreDbContext dbContext)
        {
            Console.WriteLine("Starting data seeding process...");
            Console.WriteLine($"Initializing {JeuServiceGames.Count} games from JeuService/GameEngine...");
            
            // S'assurer que les catégories existent
            Console.WriteLine("Checking categories...");
            var actionCategory = dbContext.Categories.FirstOrDefault(c => c.Name == "Action");
            var strategyCategory = dbContext.Categories.FirstOrDefault(c => c.Name == "Strategy");
            var puzzleCategory = dbContext.Categories.FirstOrDefault(c => c.Name == "Puzzle");
            
            if (actionCategory == null)
            {
                Console.WriteLine("  → Creating Action category...");
                actionCategory = new Category { Name = "Action", Description = "Action games" };
                dbContext.Categories.Add(actionCategory);
            }
            else
            {
                Console.WriteLine($"  → Action category already exists (ID: {actionCategory.Id})");
            }
            
            if (strategyCategory == null)
            {
                Console.WriteLine("  → Creating Strategy category...");
                strategyCategory = new Category { Name = "Strategy", Description = "Strategy games" };
                dbContext.Categories.Add(strategyCategory);
            }
            else
            {
                Console.WriteLine($"  → Strategy category already exists (ID: {strategyCategory.Id})");
            }
            
            if (puzzleCategory == null)
            {
                Console.WriteLine("  → Creating Puzzle category...");
                puzzleCategory = new Category { Name = "Puzzle", Description = "Puzzle games" };
                dbContext.Categories.Add(puzzleCategory);
            }
            else
            {
                Console.WriteLine($"  → Puzzle category already exists (ID: {puzzleCategory.Id})");
            }
            
            Console.WriteLine("  → Saving categories...");
            dbContext.SaveChanges();
            Console.WriteLine("  ✓ Categories saved successfully!");

            // Mapper les catégories par nom pour faciliter l'accès
            var categoryMap = new Dictionary<string, Category>
            {
                { "Action", actionCategory },
                { "Strategy", strategyCategory },
                { "Puzzle", puzzleCategory }
            };

            // Créer la liste des jeux à partir de JeuServiceGames
            var gameEngineGames = JeuServiceGames.Select(kvp => new
            {
                GameType = kvp.Key,
                Title = kvp.Value.Title,
                Description = kvp.Value.Description,
                GameUrl = $"/games/{kvp.Key}",
                Category = categoryMap[kvp.Value.CategoryName],
                Price = kvp.Value.Price
            }).ToArray();

            // Supprimer TOUS les jeux qui ne correspondent pas aux jeux du GameEngine (JeuService)
            // Liste des URLs valides des jeux du GameEngine
            Console.WriteLine("Checking existing games...");
            var validGameUrls = gameEngineGames.Select(g => g.GameUrl).ToArray();
            
            var allGames = dbContext.Games.ToList();
            Console.WriteLine($"  → Found {allGames.Count} existing game(s) in database");
            var gamesToRemove = allGames.Where(g => !validGameUrls.Contains(g.GameUrl)).ToList();
            
            if (gamesToRemove.Any())
            {
                Console.WriteLine($"  → Removing {gamesToRemove.Count} old games that don't match GameEngine games:");
                foreach (var oldGame in gamesToRemove)
                {
                    Console.WriteLine($"    - Removing: {oldGame.Title} (ID: {oldGame.Id}, URL: {oldGame.GameUrl})");
                    dbContext.Games.Remove(oldGame);
                }
                dbContext.SaveChanges();
                Console.WriteLine($"  ✓ Removed {gamesToRemove.Count} old games.");
            }

            Console.WriteLine("Adding/updating games...");
            bool anyNewGames = false;
            foreach (var game in gameEngineGames)
            {
                var existingGame = dbContext.Games.FirstOrDefault(g => g.GameUrl == game.GameUrl);
                if (existingGame == null)
                {
                    var newGame = new Game
                    {
                        Title = game.Title,
                        Description = game.Description,
                        Price = game.Price,
                        ImageUrl = $"/images/{game.GameUrl.Replace("/games/", "")}.jpg",
                        GameUrl = game.GameUrl,
                        CategoryId = game.Category.Id
                    };
                    dbContext.Games.Add(newGame);
                    Console.WriteLine($"  → Adding game: {game.Title} (URL: {game.GameUrl}, Type: {game.GameType}, Category: {game.Category.Name})");
                    anyNewGames = true;
                }
                else
                {
                    // Mettre à jour le jeu existant si nécessaire
                    if (existingGame.Title != game.Title || existingGame.Description != game.Description)
                    {
                        existingGame.Title = game.Title;
                        existingGame.Description = game.Description;
                        existingGame.Price = game.Price;
                        existingGame.CategoryId = game.Category.Id;
                        Console.WriteLine($"  → Updating game: {game.Title} (ID: {existingGame.Id}, URL: {existingGame.GameUrl})");
                        anyNewGames = true;
                    }
                    else
                    {
                        Console.WriteLine($"  → Game already exists: {game.Title} (ID: {existingGame.Id}, URL: {existingGame.GameUrl})");
                    }
                }
            }
            
            if (anyNewGames)
            {
                dbContext.SaveChanges();
                Console.WriteLine("✓ GameStoreService: GameEngine games initialized successfully!");
            }
            else
            {
                Console.WriteLine("✓ GameStoreService: All GameEngine games already exist in database.");
            }
            
            // Afficher tous les jeux disponibles
            var finalGames = dbContext.Games.ToList();
            Console.WriteLine($"✓ GameStoreService: Total games in database: {finalGames.Count}");
            foreach (var game in finalGames)
            {
                Console.WriteLine($"  - {game.Title} (ID: {game.Id}, URL: {game.GameUrl}, Category: {game.CategoryId})");
            }
        }
    }
}
