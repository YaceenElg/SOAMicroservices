using System.Runtime.Serialization;
using System.ServiceModel;
using FRONT.Models;

namespace FRONT.Services
{
    public class GameStoreServiceClient
    {
        private readonly string _serviceUrl;

        public GameStoreServiceClient(IConfiguration configuration)
        {
            _serviceUrl = configuration["ServiceUrls:GameStoreService"] ?? "http://localhost:5002/GameStoreService.svc";
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            System.ServiceModel.IClientChannel? clientChannel = null;
            ChannelFactory<IGameStoreServiceSOAP>? factory = null;
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
                
                factory = new ChannelFactory<IGameStoreServiceSOAP>(binding, endpoint);
                var channel = factory.CreateChannel();
                clientChannel = (System.ServiceModel.IClientChannel)channel;
                
                if (clientChannel.State != System.ServiceModel.CommunicationState.Opened)
                {
                    clientChannel.Open();
                }

                var response = channel.GetAllCategories();
                Console.WriteLine($"[GameStoreServiceClient] GetAllCategoriesAsync: Received response, Categories count = {response?.Categories?.Count ?? 0}");
                
                if (response?.Categories == null || response.Categories.Count == 0)
                {
                    Console.WriteLine($"[GameStoreServiceClient] GetAllCategoriesAsync: No categories in response");
                    return new List<Category>();
                }

                var categories = response.Categories.Select(c => new Category
                {
                    Id = c.Id,
                    Name = c.Name ?? string.Empty,
                    Description = c.Description ?? string.Empty
                }).ToList();
                
                Console.WriteLine($"[GameStoreServiceClient] GetAllCategoriesAsync: Successfully mapped {categories.Count} categories");
                foreach (var cat in categories)
                {
                    Console.WriteLine($"[GameStoreServiceClient]   - Category: Id={cat.Id}, Name='{cat.Name}', Description='{cat.Description}'");
                }
                
                return categories;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameStoreServiceClient] GetAllCategoriesAsync Error: {ex.Message}");
                Console.WriteLine($"[GameStoreServiceClient] GetAllCategoriesAsync StackTrace: {ex.StackTrace}");
                return new List<Category>();
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

        public async Task<List<Game>> GetGamesByCategoryAsync(int categoryId)
        {
            try
            {
                var binding = new BasicHttpBinding();
                var endpoint = new EndpointAddress(_serviceUrl);
                
                using var factory = new ChannelFactory<IGameStoreServiceSOAP>(binding, endpoint);
                var channel = factory.CreateChannel();

                var response = channel.GetGamesByCategory(categoryId);
                return response.Games.Select(g => new Game
                {
                    Id = g.Id,
                    Title = g.Title,
                    Description = g.Description,
                    Price = g.Price,
                    ImageUrl = g.ImageUrl,
                    GameUrl = g.GameUrl,
                    CategoryId = g.CategoryId,
                    Category = g.Category != null ? new Category
                    {
                        Id = g.Category.Id,
                        Name = g.Category.Name,
                        Description = g.Category.Description
                    } : null
                }).ToList();
            }
            catch
            {
                return new List<Game>();
            }
        }

        public async Task<Game?> GetGameAsync(int id)
        {
            try
            {
                var binding = new BasicHttpBinding();
                var endpoint = new EndpointAddress(_serviceUrl);
                
                using var factory = new ChannelFactory<IGameStoreServiceSOAP>(binding, endpoint);
                var channel = factory.CreateChannel();

                var game = channel.GetGame(id);
                if (game == null) return null;

                return new Game
                {
                    Id = game.Id,
                    Title = game.Title,
                    Description = game.Description,
                    Price = game.Price,
                    ImageUrl = game.ImageUrl,
                    GameUrl = game.GameUrl,
                    CategoryId = game.CategoryId,
                    Category = game.Category != null ? new Category
                    {
                        Id = game.Category.Id,
                        Name = game.Category.Name,
                        Description = game.Category.Description
                    } : null
                };
            }
            catch
            {
                return null;
            }
        }
    }

    [ServiceContract(Namespace = "http://tempuri.org/")]
    public interface IGameStoreServiceSOAP
    {
        [OperationContract]
        GetAllCategoriesResponse GetAllCategories();

        [OperationContract]
        GetAllGamesResponse GetGamesByCategory(int categoryId);

        [OperationContract]
        GameResponse? GetGame(int id);
    }

    [DataContract(Namespace = "http://tempuri.org/")]
    public class CategoryResponse
    {
        [DataMember(Order = 0)]
        public int Id { get; set; }

        [DataMember(Order = 1)]
        public string Name { get; set; } = string.Empty;

        [DataMember(Order = 2)]
        public string Description { get; set; } = string.Empty;
    }

    [DataContract(Namespace = "http://tempuri.org/")]
    public class GetAllCategoriesResponse
    {
        [DataMember(Order = 0)]
        public List<CategoryResponse> Categories { get; set; } = new();
    }

    [DataContract(Namespace = "http://tempuri.org/")]
    public class GameResponse
    {
        [DataMember(Order = 0)]
        public int Id { get; set; }

        [DataMember(Order = 1)]
        public string Title { get; set; } = string.Empty;

        [DataMember(Order = 2)]
        public string Description { get; set; } = string.Empty;

        [DataMember(Order = 3)]
        public decimal Price { get; set; }

        [DataMember(Order = 4)]
        public string ImageUrl { get; set; } = string.Empty;

        [DataMember(Order = 5)]
        public string GameUrl { get; set; } = string.Empty;

        [DataMember(Order = 6)]
        public int CategoryId { get; set; }

        [DataMember(Order = 7)]
        public CategoryResponse? Category { get; set; }

        [DataMember(Order = 8)]
        public DateTime CreatedAt { get; set; }
    }

    [DataContract(Namespace = "http://tempuri.org/")]
    public class GetAllGamesResponse
    {
        [DataMember(Order = 0)]
        public List<GameResponse> Games { get; set; } = new();
    }
}
