using System.Runtime.Serialization;
using System.ServiceModel;

namespace JeuService.SOAP
{
    [ServiceContract]
    public interface IJeuServiceSOAP
    {
        [OperationContract]
        GameInfo? GetGameInfo(int gameId);

        [OperationContract]
        RecordInjuryStatusResponse RecordInjuryStatus(int gameId, int playerId, string status);

        [OperationContract]
        GetInjuryStatusResponse GetInjuryStatus(int gameId, int playerId);

        // Developed games operations
        [OperationContract]
        DevelopedGameResponse AddDevelopedGame(DevelopedGameRequest game);

        [OperationContract]
        DevelopedGameResponse? GetDevelopedGame(int id);

        [OperationContract]
        GetAllDevelopedGamesResponse GetAllDevelopedGames();

        [OperationContract]
        DevelopedGameResponse? UpdateDevelopedGame(int id, DevelopedGameRequest game);

        [OperationContract]
        bool DeleteDevelopedGame(int id);

        // Game Engine operations
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
    public class GameInfo
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
    }

    [DataContract(Namespace = "http://tempuri.org/")]
    public class RecordInjuryStatusResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string Message { get; set; } = string.Empty;

        [DataMember]
        public int InjuryStatusId { get; set; }
    }

    [DataContract]
    public class GetInjuryStatusResponse
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int GameId { get; set; }

        [DataMember]
        public int PlayerId { get; set; }

        [DataMember]
        public string Status { get; set; } = string.Empty;

        [DataMember]
        public DateTime RecordedAt { get; set; }
    }

    [DataContract(Namespace = "http://tempuri.org/")]
    public class DevelopedGameRequest
    {
        [DataMember(Order = 0)]
        public string Title { get; set; } = string.Empty;

        [DataMember(Order = 1)]
        public string Description { get; set; } = string.Empty;

        [DataMember(Order = 2)]
        public string Developer { get; set; } = string.Empty;

        [DataMember(Order = 3)]
        public string Version { get; set; } = string.Empty;

        [DataMember(Order = 4)]
        public string GameUrl { get; set; } = string.Empty;

        [DataMember(Order = 5)]
        public DateTime ReleaseDate { get; set; }

        [DataMember(Order = 6)]
        public bool IsActive { get; set; } = true;
    }

    [DataContract(Namespace = "http://tempuri.org/")]
    public class DevelopedGameResponse
    {
        [DataMember(Order = 0)]
        public int Id { get; set; }

        [DataMember(Order = 1)]
        public string Title { get; set; } = string.Empty;

        [DataMember(Order = 2)]
        public string Description { get; set; } = string.Empty;

        [DataMember(Order = 3)]
        public string Developer { get; set; } = string.Empty;

        [DataMember(Order = 4)]
        public string Version { get; set; } = string.Empty;

        [DataMember(Order = 5)]
        public string GameUrl { get; set; } = string.Empty;

        [DataMember(Order = 6)]
        public DateTime ReleaseDate { get; set; }

        [DataMember(Order = 7)]
        public bool IsActive { get; set; }
    }

    [DataContract(Namespace = "http://tempuri.org/")]
    public class GetAllDevelopedGamesResponse
    {
        [DataMember(Order = 0)]
        public List<DevelopedGameResponse> Games { get; set; } = new();
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
        public string GameDataJson { get; set; } = string.Empty; // Serialized JSON

        [DataMember(Order = 6)]
        public DateTime LastUpdated { get; set; }
    }
}
