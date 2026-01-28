using System.Runtime.Serialization;
using System.ServiceModel;
using GameStoreService.Services;

namespace GameStoreService.SOAP
{
    public class GameStoreServiceSOAP : IGameStoreServiceSOAP
    {
        private readonly Services.IGameStoreService _gameStoreService;

        public GameStoreServiceSOAP(Services.IGameStoreService gameStoreService)
        {
            _gameStoreService = gameStoreService;
        }

        public CategoryResponse AddCategory(CategoryRequest category)
        {
            var domainCategory = new Models.Category
            {
                Name = category.Name,
                Description = category.Description
            };

            var result = _gameStoreService.AddCategoryAsync(domainCategory).Result;
            return new CategoryResponse
            {
                Id = result.Id,
                Name = result.Name,
                Description = result.Description
            };
        }

        public CategoryResponse? GetCategory(int id)
        {
            var category = _gameStoreService.GetCategoryAsync(id).Result;
            if (category == null) return null;

            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public GetAllCategoriesResponse GetAllCategories()
        {
            var categories = _gameStoreService.GetAllCategoriesAsync().Result;
            return new GetAllCategoriesResponse
            {
                Categories = categories.Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                }).ToList()
            };
        }

        public CategoryResponse? UpdateCategory(int id, CategoryRequest category)
        {
            var domainCategory = new Models.Category
            {
                Name = category.Name,
                Description = category.Description
            };

            var result = _gameStoreService.UpdateCategoryAsync(id, domainCategory).Result;
            if (result == null) return null;

            return new CategoryResponse
            {
                Id = result.Id,
                Name = result.Name,
                Description = result.Description
            };
        }

        public bool DeleteCategory(int id)
        {
            return _gameStoreService.DeleteCategoryAsync(id).Result;
        }

        public GameResponse AddGame(GameRequest game)
        {
            var domainGame = new Models.Game
            {
                Title = game.Title,
                Description = game.Description,
                Price = game.Price,
                ImageUrl = game.ImageUrl,
                GameUrl = game.GameUrl,
                CategoryId = game.CategoryId
            };

            var result = _gameStoreService.AddGameAsync(domainGame).Result;
            return new GameResponse
            {
                Id = result.Id,
                Title = result.Title,
                Description = result.Description,
                Price = result.Price,
                ImageUrl = result.ImageUrl,
                GameUrl = result.GameUrl,
                CategoryId = result.CategoryId,
                Category = result.Category != null ? new CategoryResponse
                {
                    Id = result.Category.Id,
                    Name = result.Category.Name,
                    Description = result.Category.Description
                } : null,
                CreatedAt = result.CreatedAt
            };
        }

        public GameResponse? GetGame(int id)
        {
            var game = _gameStoreService.GetGameAsync(id).Result;
            if (game == null) return null;

            return new GameResponse
            {
                Id = game.Id,
                Title = game.Title,
                Description = game.Description,
                Price = game.Price,
                ImageUrl = game.ImageUrl,
                GameUrl = game.GameUrl,
                CategoryId = game.CategoryId,
                Category = game.Category != null ? new CategoryResponse
                {
                    Id = game.Category.Id,
                    Name = game.Category.Name,
                    Description = game.Category.Description
                } : null,
                CreatedAt = game.CreatedAt
            };
        }

        public GetAllGamesResponse GetAllGames()
        {
            var games = _gameStoreService.GetAllGamesAsync().Result;
            return new GetAllGamesResponse
            {
                Games = games.Select(g => new GameResponse
                {
                    Id = g.Id,
                    Title = g.Title,
                    Description = g.Description,
                    Price = g.Price,
                    ImageUrl = g.ImageUrl,
                    GameUrl = g.GameUrl,
                    CategoryId = g.CategoryId,
                    Category = g.Category != null ? new CategoryResponse
                    {
                        Id = g.Category.Id,
                        Name = g.Category.Name,
                        Description = g.Category.Description
                    } : null,
                    CreatedAt = g.CreatedAt
                }).ToList()
            };
        }

        public GetAllGamesResponse GetGamesByCategory(int categoryId)
        {
            var games = _gameStoreService.GetGamesByCategoryAsync(categoryId).Result;
            return new GetAllGamesResponse
            {
                Games = games.Select(g => new GameResponse
                {
                    Id = g.Id,
                    Title = g.Title,
                    Description = g.Description,
                    Price = g.Price,
                    ImageUrl = g.ImageUrl,
                    GameUrl = g.GameUrl,
                    CategoryId = g.CategoryId,
                    Category = g.Category != null ? new CategoryResponse
                    {
                        Id = g.Category.Id,
                        Name = g.Category.Name,
                        Description = g.Category.Description
                    } : null,
                    CreatedAt = g.CreatedAt
                }).ToList()
            };
        }

        public GameResponse? UpdateGame(int id, GameRequest game)
        {
            var domainGame = new Models.Game
            {
                Title = game.Title,
                Description = game.Description,
                Price = game.Price,
                ImageUrl = game.ImageUrl,
                GameUrl = game.GameUrl,
                CategoryId = game.CategoryId
            };

            var result = _gameStoreService.UpdateGameAsync(id, domainGame).Result;
            if (result == null) return null;

            return new GameResponse
            {
                Id = result.Id,
                Title = result.Title,
                Description = result.Description,
                Price = result.Price,
                ImageUrl = result.ImageUrl,
                GameUrl = result.GameUrl,
                CategoryId = result.CategoryId,
                Category = result.Category != null ? new CategoryResponse
                {
                    Id = result.Category.Id,
                    Name = result.Category.Name,
                    Description = result.Category.Description
                } : null,
                CreatedAt = result.CreatedAt
            };
        }

        public bool DeleteGame(int id)
        {
            return _gameStoreService.DeleteGameAsync(id).Result;
        }
    }
}
