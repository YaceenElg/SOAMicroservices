using System.Runtime.Serialization;
using System.ServiceModel;
using UserService.Services;

namespace UserService.SOAP
{
    public class UserServiceSOAP : IUserServiceSOAP
    {
        private readonly Services.IUserService _userService;

        public UserServiceSOAP(Services.IUserService userService)
        {
            _userService = userService;
        }

        public UserResponse AddUser(UserRequest user)
        {
            try
            {
                var domainUser = new Models.User
                {
                    Username = user.Username,
                    Email = user.Email,
                    Password = user.Password
                };

                var result = _userService.AddUserAsync(domainUser).Result;
                return new UserResponse
                {
                    Id = result.Id,
                    Username = result.Username,
                    Email = result.Email,
                    Password = result.Password,
                    CreatedAt = result.CreatedAt
                };
            }
            catch (AggregateException ae)
            {
                // Unwrap AggregateException from .Result
                var innerEx = ae.InnerException ?? ae;
                if (innerEx is InvalidOperationException)
                {
                    throw new FaultException<string>(innerEx.Message, new FaultReason(innerEx.Message));
                }
                throw new FaultException<string>($"Error adding user: {innerEx.Message}", new FaultReason(innerEx.Message));
            }
            catch (InvalidOperationException ex)
            {
                throw new FaultException<string>(ex.Message, new FaultReason(ex.Message));
            }
            catch (Exception ex)
            {
                throw new FaultException<string>($"Error adding user: {ex.Message}", new FaultReason(ex.Message));
            }
        }

        public UserResponse? GetUser(int id)
        {
            var user = _userService.GetUserAsync(id).Result;
            if (user == null) return null;

            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Password = user.Password,
                CreatedAt = user.CreatedAt
            };
        }

        public UserResponse? GetUserByUsername(string username)
        {
            try
            {
                var user = _userService.GetUserByUsernameAsync(username).Result;
                if (user == null)
                {
                    Console.WriteLine($"GetUserByUsername: User '{username}' not found");
                    return null;
                }

                Console.WriteLine($"GetUserByUsername: Found user '{username}' with Id={user.Id}, Email={user.Email}");
                return new UserResponse
                {
                    Id = user.Id,
                    Username = user.Username ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Password = user.Password ?? string.Empty,
                    CreatedAt = user.CreatedAt
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserByUsername error: {ex.Message}");
                throw;
            }
        }

        public GetAllUsersResponse GetAllUsers()
        {
            var users = _userService.GetAllUsersAsync().Result;
            return new GetAllUsersResponse
            {
                Users = users.Select(u => new UserResponse
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Password = u.Password,
                    CreatedAt = u.CreatedAt
                }).ToList()
            };
        }

        public UserResponse? UpdateUser(int id, UserRequest user)
        {
            var domainUser = new Models.User
            {
                Username = user.Username,
                Email = user.Email,
                Password = user.Password
            };

            var result = _userService.UpdateUserAsync(id, domainUser).Result;
            if (result == null) return null;

            return new UserResponse
            {
                Id = result.Id,
                Username = result.Username,
                Email = result.Email,
                Password = result.Password,
                CreatedAt = result.CreatedAt
            };
        }

        public bool DeleteUser(int id)
        {
            return _userService.DeleteUserAsync(id).Result;
        }

        public bool Authenticate(string username, string password)
        {
            try
            {
                Console.WriteLine($"Authenticate called for username: '{username}'");
                var result = _userService.AuthenticateAsync(username, password).Result;
                Console.WriteLine($"Authenticate result for '{username}': {result}");
                return result;
            }
            catch (AggregateException ae)
            {
                // Unwrap AggregateException from .Result
                var innerEx = ae.InnerException ?? ae;
                Console.WriteLine($"Authenticate AggregateException for '{username}': {innerEx.Message}");
                throw new FaultException<string>($"Authentication error: {innerEx.Message}", new FaultReason(innerEx.Message));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authenticate Exception for '{username}': {ex.Message}");
                throw new FaultException<string>($"Authentication error: {ex.Message}", new FaultReason(ex.Message));
            }
        }
    }
}
