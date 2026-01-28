using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GameStoreService.Data
{
    public class GameStoreDbContextFactory : IDesignTimeDbContextFactory<GameStoreDbContext>
    {
        public GameStoreDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GameStoreDbContext>();
            optionsBuilder.UseSqlServer("Server=DESKTOP-15P65VR\\SQLEXPRESS;Database=GameStoreServiceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");

            return new GameStoreDbContext(optionsBuilder.Options);
        }
    }
}
