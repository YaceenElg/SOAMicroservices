using Microsoft.EntityFrameworkCore;
using GameStoreService.Data;
using GameStoreService.Models;

namespace GameStoreService.Services
{
    public class GameStoreService : IGameStoreService
    {
        private readonly GameStoreDbContext _context;

        public GameStoreService(GameStoreDbContext context)
        {
            _context = context;
        }

        public async Task<Category> AddCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category?> GetCategoryAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category?> UpdateCategoryAsync(int id, Category category)
        {
            var existing = await _context.Categories.FindAsync(id);
            if (existing == null)
                return null;

            existing.Name = category.Name;
            existing.Description = category.Description;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Game> AddGameAsync(Game game)
        {
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
            return game;
        }

        public async Task<Game?> GetGameAsync(int id)
        {
            return await _context.Games
                .Include(g => g.Category)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<List<Game>> GetAllGamesAsync()
        {
            return await _context.Games
                .Include(g => g.Category)
                .ToListAsync();
        }

        public async Task<List<Game>> GetGamesByCategoryAsync(int categoryId)
        {
            return await _context.Games
                .Include(g => g.Category)
                .Where(g => g.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<Game?> UpdateGameAsync(int id, Game game)
        {
            var existing = await _context.Games.FindAsync(id);
            if (existing == null)
                return null;

            existing.Title = game.Title;
            existing.Description = game.Description;
            existing.Price = game.Price;
            existing.ImageUrl = game.ImageUrl;
            existing.GameUrl = game.GameUrl;
            existing.CategoryId = game.CategoryId;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteGameAsync(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
                return false;

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
