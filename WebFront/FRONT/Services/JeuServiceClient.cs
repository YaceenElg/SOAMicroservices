using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text.Json;

namespace FRONT.Services
{
    public class JeuServiceClient
    {
        private readonly string _serviceUrl;

        public JeuServiceClient(IConfiguration configuration)
        {
            _serviceUrl = configuration["ServiceUrls:JeuService"] ?? "http://localhost:5003/JeuService.svc";
        }

        public async Task<GameStateResponse> StartGameAsync(string gameType, int playerId)
        {
            System.ServiceModel.IClientChannel? clientChannel = null;
            ChannelFactory<IJeuServiceSOAP>? factory = null;
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
                
                factory = new ChannelFactory<IJeuServiceSOAP>(binding, endpoint);
                var channel = factory.CreateChannel();
                clientChannel = (System.ServiceModel.IClientChannel)channel;
                
                if (clientChannel.State != System.ServiceModel.CommunicationState.Opened)
                {
                    clientChannel.Open();
                }

                var response = channel.StartGame(gameType, playerId);
                Console.WriteLine($"[JeuServiceClient] StartGameAsync: Response received - GameId={response?.GameId}, GameType={response?.GameType}, Status={response?.Status}, Score={response?.Score}");
                Console.WriteLine($"[JeuServiceClient] StartGameAsync: GameDataJson length={response?.GameDataJson?.Length ?? 0}");
                
                // Ensure GameType is never null or empty
                if (response != null && string.IsNullOrWhiteSpace(response.GameType))
                {
                    Console.WriteLine($"[JeuServiceClient] StartGameAsync WARNING: GameType is null/empty, setting to requested type '{gameType}'");
                    response.GameType = gameType;
                }
                
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JeuServiceClient] StartGameAsync Error: {ex.Message}");
                throw;
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

        public async Task<GameStateResponse> PlayActionAsync(int gameId, string action, int playerId)
        {
            System.ServiceModel.IClientChannel? clientChannel = null;
            ChannelFactory<IJeuServiceSOAP>? factory = null;
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
                
                factory = new ChannelFactory<IJeuServiceSOAP>(binding, endpoint);
                var channel = factory.CreateChannel();
                clientChannel = (System.ServiceModel.IClientChannel)channel;
                
                if (clientChannel.State != System.ServiceModel.CommunicationState.Opened)
                {
                    clientChannel.Open();
                }

                Console.WriteLine($"[JeuServiceClient] PlayActionAsync: Calling PlayAction with gameId={gameId}, action='{action}', playerId={playerId}");
                var response = channel.PlayAction(gameId, action, playerId);
                
                Console.WriteLine($"[JeuServiceClient] PlayActionAsync: Response received - GameId={response?.GameId}, GameType={response?.GameType}, Status={response?.Status}, Score={response?.Score}");
                
                // Ensure GameId is never null or 0
                if (response != null)
                {
                    if (response.GameId <= 0)
                    {
                        Console.WriteLine($"[JeuServiceClient] PlayActionAsync WARNING: Response has invalid GameId={response.GameId}, using requested gameId={gameId}");
                        response.GameId = gameId; // Preserve the requested gameId
                    }
                    
                    if (string.IsNullOrWhiteSpace(response.GameType))
                    {
                        Console.WriteLine($"[JeuServiceClient] PlayActionAsync WARNING: Response has empty GameType, cannot fix without context");
                    }
                }
                
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JeuServiceClient] PlayActionAsync Error: {ex.Message}");
                throw;
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

        public async Task<GameStateResponse> GetGameStateAsync(int gameId, int playerId)
        {
            System.ServiceModel.IClientChannel? clientChannel = null;
            ChannelFactory<IJeuServiceSOAP>? factory = null;
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
                
                factory = new ChannelFactory<IJeuServiceSOAP>(binding, endpoint);
                var channel = factory.CreateChannel();
                clientChannel = (System.ServiceModel.IClientChannel)channel;
                
                if (clientChannel.State != System.ServiceModel.CommunicationState.Opened)
                {
                    clientChannel.Open();
                }

                var response = channel.GetGameState(gameId, playerId);
                
                // Ensure GameType is never null or empty
                if (response != null && string.IsNullOrWhiteSpace(response.GameType))
                {
                    Console.WriteLine($"[JeuServiceClient] GetGameStateAsync WARNING: GameType is null/empty for GameId={gameId}, using fallback");
                    response.GameType = "tictactoe"; // Fallback
                }
                
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JeuServiceClient] GetGameStateAsync Error: {ex.Message}");
                throw;
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

        public Dictionary<string, object> ParseGameData(string gameDataJson)
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(gameDataJson) ?? new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }
    }

    [ServiceContract(Namespace = "http://tempuri.org/")]
    public interface IJeuServiceSOAP
    {
        [OperationContract]
        GameStateResponse StartGame(string gameType, int playerId);

        [OperationContract]
        GameStateResponse PlayAction(int gameId, string action, int playerId);

        [OperationContract]
        GameStateResponse GetGameState(int gameId, int playerId);

        [OperationContract]
        bool EndGame(int gameId, int playerId);
    }

    [DataContract(Namespace = "http://tempuri.org/")]
    public class GameStateResponse
    {
        [DataMember(Order = 0)]
        public int GameId { get; set; }

        [DataMember(Order = 1)]
        public string GameType { get; set; } = string.Empty;

        [DataMember(Order = 2)]
        public string Status { get; set; } = string.Empty;

        [DataMember(Order = 3)]
        public int Score { get; set; }

        [DataMember(Order = 4)]
        public int Level { get; set; }

        [DataMember(Order = 5)]
        public string GameDataJson { get; set; } = string.Empty;

        [DataMember(Order = 6)]
        public DateTime LastUpdated { get; set; }
    }
}
