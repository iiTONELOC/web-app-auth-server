using System.Text.Json.Serialization;


namespace WebAppAuthenticationServer.Models;


interface IMessage<in T>
{
}

public class _Message<T> : IMessage<T>
{
    [JsonPropertyName("error_code")]
    public int Code { get; set; }
    [JsonPropertyName("error_message")]
    public virtual T? Message { get; set; }
}

public class DataValidationMessage<T>
{
    [JsonPropertyName("message")]
    public _Message<T>? Message = new _Message<T>();
    [JsonPropertyName("is_valid")]
    public bool IsValid { get; set; }
}

// not really generic but creating a new file seems silly for one class
public class FieldValidationMessage
{
    [JsonPropertyName("property")]

    public string? PropertyName { get; set; }

    [JsonPropertyName("is_valid")]
    public bool IsValid { get; set; } = false;

    [JsonPropertyName("error_messages")]
    public List<_Message<string>>? ErrorMessages { get; set; }

}

public class ApiResponse<T>
{
    [JsonPropertyName("status")]
    public int StatusCode { get; set; }

    [JsonPropertyName("error")]
    public T? Error { get; set; }

    [JsonPropertyName("data")]
    public object? Data { get; set; }
}