using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserService.Data
{
    public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
    {
        public UserDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
            optionsBuilder.UseSqlServer("Server=DESKTOP-15P65VR\\SQLEXPRESS;Database=UserServiceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");

            return new UserDbContext(optionsBuilder.Options);
        }
    }
}
