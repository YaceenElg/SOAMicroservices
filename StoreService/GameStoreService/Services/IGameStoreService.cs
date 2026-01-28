using GameStoreService.Models;

namespace GameStoreService.Services
{
    public interface IGameStoreService
    {
        // Category operations
        Task<Category> AddCategoryAsync(Category category);
        Task<Category?> GetCategoryAsync(int id);
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category?> UpdateCategoryAsync(int id, Category category);
        Task<bool> DeleteCategoryAsync(int id);

        // Game operations
        Task<Game> AddGameAsync(Game game);
        Task<Game?> GetGameAsync(int id);
        Task<List<Game>> GetAllGamesAsync();
        Task<List<Game>> GetGamesByCategoryAsync(int categoryId);
        Task<Game?> UpdateGameAsync(int id, Game game);
        Task<bool> DeleteGameAsync(int id);
    }
}
