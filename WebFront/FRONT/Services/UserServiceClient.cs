using System.Runtime.Serialization;
using System.ServiceModel;
using FRONT.Models;
using System.ServiceModel.Channels;

namespace FRONT.Services
{
    public class UserServiceClient
    {
        private readonly string _serviceUrl;

        public UserServiceClient(IConfiguration configuration)
        {
            _serviceUrl = configuration["ServiceUrls:UserService"] ?? "http://localhost:5001/UserService.svc";
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            System.ServiceModel.IClientChannel? clientChannel = null;
            ChannelFactory<IUserServiceSOAP>? factory = null;
            try
            {
                var binding = new BasicHttpBinding
                {
                    MaxReceivedMessageSize = 2147483647,
                    ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas
                    {
                        MaxArrayLength = 2147483647,
                        MaxStringContentLength = 2147483647
                    }
                };
                var endpoint = new EndpointAddress(_serviceUrl);
                
                factory = new ChannelFactory<IUserServiceSOAP>(binding, endpoint);
                var channel = factory.CreateChannel();
                clientChannel = (System.ServiceModel.IClientChannel)channel;
                
                if (clientChannel.State != System.ServiceModel.CommunicationState.Opened)
                {
                    clientChannel.Open();
                }

                bool result = channel.Authenticate(username ?? string.Empty, password ?? string.Empty);
                Console.WriteLine($"[UserServiceClient] AuthenticateAsync: Result for '{username}' = {result} (type: {result.GetType()})");
                System.Diagnostics.Debug.WriteLine($"[UserServiceClient] AuthenticateAsync: Result for '{username}' = {result}");
                
                return result;
            }
            catch (FaultException<string> fe)
            {
                Console.WriteLine($"[UserServiceClient] Authentication SOAP fault: {fe.Detail}");
                System.Diagnostics.Debug.WriteLine($"Authentication SOAP fault: {fe.Detail}");
                throw new Exception($"Authentication error: {fe.Detail}");
            }
            catch (FaultException fe)
            {
                Console.WriteLine($"[UserServiceClient] Authentication SOAP fault: {fe.Message}");
                System.Diagnostics.Debug.WriteLine($"Authentication SOAP fault: {fe.Message}");
                throw new Exception($"Authentication error: {fe.Message}");
            }
            catch (EndpointNotFoundException ex)
            {
                Console.WriteLine($"[UserServiceClient] EndpointNotFoundException: {ex.Message}");
                throw new Exception($"UserService is not available at {_serviceUrl}. Please ensure the service is running on port 5001.");
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                Console.WriteLine($"[UserServiceClient] CommunicationException: {ex.Message}");
                throw new Exception($"Cannot connect to UserService at {_serviceUrl}. Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserServiceClient] Authentication error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Authentication error: {ex.Message}");
                throw new Exception($"Authentication failed: {ex.Message}");
            }
            finally
            {
                if (clientChannel != null)
                {
                    try
                    {
                        if (clientChannel.State == System.ServiceModel.CommunicationState.Opened)
                        {
                            clientChannel.Close();
                        }
                        else if (clientChannel.State == System.ServiceModel.CommunicationState.Faulted)
                        {
                            clientChannel.Abort();
                        }
                    }
                    catch
                    {
                        clientChannel.Abort();
                    }
                }
            }
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            System.ServiceModel.IClientChannel? clientChannel = null;
            ChannelFactory<IUserServiceSOAP>? factory = null;
            try
            {
                var binding = new BasicHttpBinding
                {
                    MaxReceivedMessageSize = 2147483647,
                    ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas
                    {
                        MaxArrayLength = 2147483647,
                        MaxStringContentLength = 2147483647
                    }
                };
                var endpoint = new EndpointAddress(_serviceUrl);
                
                factory = new ChannelFactory<IUserServiceSOAP>(binding, endpoint);
                var channel = factory.CreateChannel();
                clientChannel = (System.ServiceModel.IClientChannel)channel;
                
                if (clientChannel.State != System.ServiceModel.CommunicationState.Opened)
                {
                    clientChannel.Open();
                }

                var user = channel.GetUserByUsername(username ?? string.Empty);
                
                // Debug: Log raw SOAP response details
                if (user != null)
                {
                    Console.WriteLine($"[UserServiceClient] GetUserByUsernameAsync: Raw SOAP response received");
                    Console.WriteLine($"[UserServiceClient]   - Id: {user.Id} (type: {user.Id.GetType()})");
                    Console.WriteLine($"[UserServiceClient]   - Username: '{user.Username}' (length: {user.Username?.Length ?? 0})");
                    Console.WriteLine($"[UserServiceClient]   - Email: '{user.Email}' (length: {user.Email?.Length ?? 0})");
                    Console.WriteLine($"[UserServiceClient]   - Password: '{(string.IsNullOrEmpty(user.Password) ? "empty" : "***")}' (length: {user.Password?.Length ?? 0})");
                    Console.WriteLine($"[UserServiceClient]   - CreatedAt: {user.CreatedAt}");
                }
                else
                {
                    Console.WriteLine($"[UserServiceClient] GetUserByUsernameAsync: User '{username}' is null");
                }
                
                System.Diagnostics.Debug.WriteLine($"GetUserByUsernameAsync: User for '{username}' = {(user != null ? $"Id={user.Id}, Username={user.Username}" : "null")}");
                
                if (user == null)
                {
                    return null;
                }

                // Check if values are actually populated (SOAP deserialization issue)
                if (user.Id == 0 && string.IsNullOrEmpty(user.Username) && string.IsNullOrEmpty(user.Email))
                {
                    Console.WriteLine($"[UserServiceClient] ERROR: UserResponse received but all fields are empty/default!");
                    Console.WriteLine($"[UserServiceClient] This indicates a SOAP deserialization problem.");
                    Console.WriteLine($"[UserServiceClient] The service returned data but it's not being deserialized correctly.");
                    return null;
                }

                var frontendUser = new User
                {
                    Id = user.Id,
                    Username = user.Username ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Password = user.Password ?? string.Empty
                };
                Console.WriteLine($"[UserServiceClient] GetUserByUsernameAsync: Successfully mapped to frontend User - Id={frontendUser.Id}, Username='{frontendUser.Username}', Email='{frontendUser.Email}'");
                return frontendUser;
            }
            catch (EndpointNotFoundException ex)
            {
                Console.WriteLine($"[UserServiceClient] GetUserByUsernameAsync EndpointNotFoundException: {ex.Message}");
                throw new Exception($"UserService is not available at {_serviceUrl}. Please ensure the service is running on port 5001.");
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                Console.WriteLine($"[UserServiceClient] GetUserByUsernameAsync CommunicationException: {ex.Message}");
                throw new Exception($"Cannot connect to UserService at {_serviceUrl}. Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserServiceClient] GetUserByUsernameAsync Exception: {ex.Message}");
                return null;
            }
            finally
            {
                if (clientChannel != null)
                {
                    try
                    {
                        if (clientChannel.State == System.ServiceModel.CommunicationState.Opened)
                        {
                            clientChannel.Close();
                        }
                        else if (clientChannel.State == System.ServiceModel.CommunicationState.Faulted)
                        {
                            clientChannel.Abort();
                        }
                    }
                    catch
                    {
                        clientChannel.Abort();
                    }
                }
                factory?.Close();
            }
        }

        public async Task<User?> RegisterAsync(string username, string email, string password)
        {
            try
            {
                var binding = new BasicHttpBinding
                {
                    MaxReceivedMessageSize = 2147483647,
                    ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas
                    {
                        MaxArrayLength = 2147483647,
                        MaxStringContentLength = 2147483647
                    }
                };
                var endpoint = new EndpointAddress(_serviceUrl);
                
                using var factory = new ChannelFactory<IUserServiceSOAP>(binding, endpoint);
                var channel = factory.CreateChannel();
                ((System.ServiceModel.IClientChannel)channel).Open();

                var userRequest = new UserRequest
                {
                    Username = username,
                    Email = email,
                    Password = password
                };

                var user = channel.AddUser(userRequest);
                ((System.ServiceModel.IClientChannel)channel).Close();
                
                if (user == null) return null;

                return new User
                {
                    Id = user.Id,
                    Username = user.Username ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Password = user.Password ?? string.Empty
                };
            }
            catch (FaultException<string> fe)
            {
                // SOAP fault exception - check if it's a duplicate user error
                if (fe.Detail?.Contains("already exists") == true)
                {
                    throw new InvalidOperationException(fe.Detail);
                }
                throw new Exception(fe.Detail ?? fe.Message);
            }
            catch (FaultException fe)
            {
                // SOAP fault exception - rethrow with message
                throw new Exception(fe.Message);
            }
            catch (EndpointNotFoundException ex)
            {
                // Service not available
                throw new Exception($"UserService is not available at {_serviceUrl}. Please ensure the service is running on port 5001.");
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                // Connection error
                throw new Exception($"Cannot connect to UserService at {_serviceUrl}. Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Other exceptions - check if it's a duplicate user error
                if (ex.Message.Contains("already exists") || ex.InnerException?.Message.Contains("already exists") == true)
                {
                    throw new InvalidOperationException(ex.Message);
                }
                throw;
            }
        }
    }

    [ServiceContract(Namespace = "http://tempuri.org/")]
    public interface IUserServiceSOAP
    {
        [OperationContract]
        bool Authenticate(string username, string password);

        [OperationContract]
        UserResponse? GetUserByUsername(string username);

        [OperationContract]
        UserResponse AddUser(UserRequest user);
    }

    [DataContract(Namespace = "http://tempuri.org/")]
    public class UserRequest
    {
        [DataMember(Order = 0)]
        public string Username { get; set; } = string.Empty;

        [DataMember(Order = 1)]
        public string Email { get; set; } = string.Empty;

        [DataMember(Order = 2)]
        public string Password { get; set; } = string.Empty;
    }

    [DataContract(Namespace = "http://tempuri.org/")]
    public class UserResponse
    {
        [DataMember(Order = 0)]
        public int Id { get; set; }

        [DataMember(Order = 1)]
        public string Username { get; set; } = string.Empty;

        [DataMember(Order = 2)]
        public string Email { get; set; } = string.Empty;

        [DataMember(Order = 3)]
        public string Password { get; set; } = string.Empty;

        [DataMember(Order = 4)]
        public DateTime CreatedAt { get; set; }
    }
}
