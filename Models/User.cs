using MongoDB.Bson;
using System.Text.Json.Serialization;
using WebAppAuthenticationServer.Utils;
using MongoDB.Bson.Serialization.Attributes;

// using static WebAppAuthenticationServer.Utils.InputValidation;

namespace WebAppAuthenticationServer.Models;

public interface IUser
{
    string? Id { get; set; }
    string? Username { get; set; }
    string? Email { get; set; }
    string? Password { get; set; }
}

public class CreateUserInfo : IUser
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class LoginUserInfo
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class User : IUser
{
    private string? _password;
    private string? _passSalt;

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonIgnore]
    public string? Id { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }


    [JsonIgnore]
    public string? Password
    {
        get => _password;
        set => _password = value.Contains("HASH$") ? value : SetPassword(value);
    }

    [JsonIgnore]
    public string? PassSalt { get; set; }

    private string SetPassword(string? password)
    {
        var passHash = PassHash.HashPassword(password!, out string salt);
        PassSalt = salt;
        return passHash;
    }
}
