using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using JeuService.SOAP;

namespace JeuService.Services
{
    public class GameStoreClient : IGameStoreClient
    {
        private readonly string _gameStoreServiceUrl;

        public GameStoreClient(IConfiguration configuration)
        {
            _gameStoreServiceUrl = configuration["GameStoreServiceUrl"] ?? "http://localhost:5002/GameStoreService.svc";
        }

        public async Task<GameInfo?> GetGameAsync(int gameId)
        {
            try
            {
                var binding = new BasicHttpBinding();
                var endpoint = new EndpointAddress(_gameStoreServiceUrl);
                
                using var factory = new ChannelFactory<IGameStoreServiceSOAP>(binding, endpoint);
                var channel = factory.CreateChannel();

                var game = channel.GetGame(gameId);
                
                if (game == null)
                    return null;

                return new GameInfo
                {
                    Id = game.Id,
                    Title = game.Title,
                    Description = game.Description,
                    Price = game.Price,
                    ImageUrl = game.ImageUrl,
                    GameUrl = game.GameUrl
                };
            }
            catch
            {
                return null;
            }
        }
    }

    // Proxy interface matching GameStoreService SOAP contract
    [ServiceContract]
    public interface IGameStoreServiceSOAP
    {
        [OperationContract]
        GameResponse? GetGame(int id);
    }

    [DataContract]
    public class GameResponse
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Title { get; set; } = string.Empty;

        [DataMember]
        public string Description { get; set; } = string.Empty;

        [DataMember]
        public decimal Price { get; set; }

        [DataMember]
        public string ImageUrl { get; set; } = string.Empty;

        [DataMember]
        public string GameUrl { get; set; } = string.Empty;

        [DataMember]
        public int CategoryId { get; set; }

        [DataMember]
        public CategoryResponse? Category { get; set; }

        [DataMember]
        public DateTime CreatedAt { get; set; }
    }

    [DataContract]
    public class CategoryResponse
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; } = string.Empty;

        [DataMember]
        public string Description { get; set; } = string.Empty;
    }
}
