using UserService.Models;

namespace UserService.Services
{
    public interface IUserService
    {
        Task<User> AddUserAsync(User user);
        Task<User?> GetUserAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<List<User>> GetAllUsersAsync();
        Task<User?> UpdateUserAsync(int id, User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> AuthenticateAsync(string username, string password);
    }
}
