using System.Runtime.Serialization;
using System.ServiceModel;

namespace GameStoreService.SOAP
{
    [ServiceContract(Namespace = "http://tempuri.org/")]
    public interface IGameStoreServiceSOAP
    {
        // Category operations
        [OperationContract]
        CategoryResponse AddCategory(CategoryRequest category);

        [OperationContract]
        CategoryResponse? GetCategory(int id);

        [OperationContract]
        GetAllCategoriesResponse GetAllCategories();

        [OperationContract]
        CategoryResponse? UpdateCategory(int id, CategoryRequest category);

        [OperationContract]
        bool DeleteCategory(int id);

        // Game operations
        [OperationContract]
        GameResponse AddGame(GameRequest game);

        [OperationContract]
        GameResponse? GetGame(int id);

        [OperationContract]
        GetAllGamesResponse GetAllGames();

        [OperationContract]
        GetAllGamesResponse GetGamesByCategory(int categoryId);

        [OperationContract]
        GameResponse? UpdateGame(int id, GameRequest game);

        [OperationContract]
        bool DeleteGame(int id);
    }

    [DataContract(Namespace = "http://tempuri.org/")]
    public class CategoryRequest
    {
        [DataMember(Order = 0)]
        public string Name { get; set; } = string.Empty;

        [DataMember(Order = 1)]
        public string Description { get; set; } = string.Empty;
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
    public class GameRequest
    {
        [DataMember(Order = 0)]
        public string Title { get; set; } = string.Empty;

        [DataMember(Order = 1)]
        public string Description { get; set; } = string.Empty;

        [DataMember(Order = 2)]
        public decimal Price { get; set; }

        [DataMember(Order = 3)]
        public string ImageUrl { get; set; } = string.Empty;

        [DataMember(Order = 4)]
        public string GameUrl { get; set; } = string.Empty;

        [DataMember(Order = 5)]
        public int CategoryId { get; set; }
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
