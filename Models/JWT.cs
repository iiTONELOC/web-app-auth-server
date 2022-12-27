using System.Security.Claims;


namespace WebAppAuthenticationServer.Models;

public interface I_JWTContainerModel
{
    string SecretKey { get; set; }
    string SecurityAlgorithm { get; set; }
    int ExpireMinutes { get; set; }
    Claim[] Claims { get; set; }
}

public class JWTContainerModel : I_JWTContainerModel
{
    private string? _secretKey;
    private string? _securityAlgorithm;

    public string SecretKey
    {
        get => _secretKey!;
        set => _secretKey = (string)value;
    }

    public string SecurityAlgorithm
    {
        get => _securityAlgorithm!;
        set => _securityAlgorithm = (string)value;
    }

    public int ExpireMinutes { get; set; } = 60;
    public Claim[] Claims { get; set; } = new Claim[] { };
}
