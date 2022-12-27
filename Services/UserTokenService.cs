using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using WebAppAuthenticationServer.Models;

namespace WebAppAuthenticationServer.Services;

public class UserTokenService
{
    private static readonly string JWT_SECURITY_ALGORITHM = SecurityAlgorithms.HmacSha512Signature;
    private static readonly string JWT_SECRET = Environment.GetEnvironmentVariable("JWTUserKey1")!;
    private static readonly I_JWTService _jwtService = new JWTService(JWT_SECRET, JWT_SECURITY_ALGORITHM);


    public string GenerateToken(string username, string email, int expiresInMinutes = 60)
    {
        var _tokenModel = new JWTContainerModel
        {
            SecretKey = JWT_SECRET,
            SecurityAlgorithm = JWT_SECURITY_ALGORITHM,
            ExpireMinutes = expiresInMinutes,
            Claims = new Claim[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email)
            }
        };

        // create a token but validate it first

        string token = _jwtService.GenerateToken(_tokenModel);

        if (!_jwtService.IsTokenValid(token))
            throw new UnauthorizedAccessException("Token is not valid!");
        else
            return token;
    }

    public bool IsTokenValid(string token)
    {
        return _jwtService.IsTokenValid(token);
    }

    public IEnumerable<Claim> GetTokenClaims(string token)
    {
        return _jwtService.GetTokenClaims(token);
    }

    public string GetUsernameFromToken(string token)
    {
        return GetTokenClaims(token)?.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value!;
    }

    public string GetEmailFromToken(string token)
    {
        return GetTokenClaims(token)?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value!;
    }
}