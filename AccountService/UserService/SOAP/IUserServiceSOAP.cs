using System.Runtime.Serialization;
using System.ServiceModel;

namespace UserService.SOAP
{
    [ServiceContract(Namespace = "http://tempuri.org/")]
    public interface IUserServiceSOAP
    {
        [OperationContract]
        UserResponse AddUser(UserRequest user);

        [OperationContract]
        UserResponse? GetUser(int id);

        [OperationContract]
        UserResponse? GetUserByUsername(string username);

        [OperationContract]
        GetAllUsersResponse GetAllUsers();

        [OperationContract]
        UserResponse? UpdateUser(int id, UserRequest user);

        [OperationContract]
        bool DeleteUser(int id);

        [OperationContract]
        bool Authenticate(string username, string password);
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

    [DataContract]
    public class GetAllUsersResponse
    {
        [DataMember]
        public List<UserResponse> Users { get; set; } = new();
    }
}
