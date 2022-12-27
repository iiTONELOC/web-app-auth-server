using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using WebAppAuthenticationServer.Models;

namespace WebAppAuthenticationServer.Services;

public interface I_JWTService
{
    string SecretKey { get; set; }
    bool IsTokenValid(string token);
    string GenerateToken(I_JWTContainerModel model);
    IEnumerable<Claim> GetTokenClaims(string token);
}


public class JWTService : I_JWTService
{
    public string SecretKey { get; set; }
    public string SecurityAlgorithm { get; set; }

    public JWTService(string? secretKey, string? securityAlgorithm)
    {
        SecretKey = secretKey ?? Environment.GetEnvironmentVariable("JWTUserKey") ??
            throw new ArgumentNullException("A secret key is required to generate a JWT token!");

        SecurityAlgorithm = securityAlgorithm ?? SecurityAlgorithms.HmacSha512Signature;
    }

    public bool IsTokenValid(string token)
    {
        if (string.IsNullOrEmpty(token))
            throw new ArgumentException("Given token is null or empty.");

        TokenValidationParameters tokenValidationParameters = GetTokenValidationParameters();

        JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        try
        {
            ClaimsPrincipal tokenValid = jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public string GenerateToken(I_JWTContainerModel model)
    {
        if (model == null || model.Claims == null || model.Claims.Length == 0)
            throw new ArgumentException("Arguments to create token are not valid.");

        SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(model.Claims),
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(model.ExpireMinutes)),
            SigningCredentials = new SigningCredentials(GetSymmetricSecurityKey(), model.SecurityAlgorithm)
        };

        JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        SecurityToken securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
        string token = jwtSecurityTokenHandler.WriteToken(securityToken);

        return token;
    }

    public IEnumerable<Claim> GetTokenClaims(string token)
    {
        if (string.IsNullOrEmpty(token))
            throw new ArgumentException("Given token is null or empty.");

        TokenValidationParameters tokenValidationParameters = GetTokenValidationParameters();

        JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        try
        {
            ClaimsPrincipal tokenValid = jwtSecurityTokenHandler
            .ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
            return tokenValid.Claims;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private SecurityKey GetSymmetricSecurityKey()
    {
        byte[] symmetricKey = Convert.FromBase64String(
            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(SecretKey)));

        return new SymmetricSecurityKey(symmetricKey);
    }

    private TokenValidationParameters GetTokenValidationParameters()
    {
        return new TokenValidationParameters()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = GetSymmetricSecurityKey()
        };
    }
}

