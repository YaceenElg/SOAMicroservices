using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;

namespace UserService.Services
{
    public class UserService : IUserService
    {
        private readonly UserDbContext _context;

        public UserService(UserDbContext context)
        {
            _context = context;
        }

        public async Task<User> AddUserAsync(User user)
        {
            // Check if username already exists
            var existingUser = await GetUserByUsernameAsync(user.Username);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"Username '{user.Username}' already exists.");
            }

            // Check if email already exists
            var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingEmail != null)
            {
                throw new InvalidOperationException($"Email '{user.Email}' already exists.");
            }

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                // Catch database constraint violations
                if (ex.InnerException?.Message.Contains("UNIQUE") == true || 
                    ex.InnerException?.Message.Contains("duplicate") == true)
                {
                    throw new InvalidOperationException($"Username or email already exists.");
                }
                throw;
            }
        }

        public async Task<User?> GetUserAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> UpdateUserAsync(int id, User user)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return null;

            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.Password = user.Password;

            await _context.SaveChangesAsync();
            return existingUser;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            var user = await GetUserByUsernameAsync(username);
            if (user == null)
            {
                return false;
            }

            // Compare passwords (case-sensitive, trim whitespace)
            var passwordMatch = user.Password?.Trim() == password?.Trim();
            
            // Debug logging (remove in production)
            if (!passwordMatch)
            {
                Console.WriteLine($"Authentication failed for user '{username}'. Expected: '{user.Password}', Received: '{password}'");
            }
            
            return passwordMatch;
        }
    }
}
