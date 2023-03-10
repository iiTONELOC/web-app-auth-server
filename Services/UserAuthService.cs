using WebAppAuthenticationServer.Models;

namespace WebAppAuthenticationServer.Services;

public static class UserAuthService
{
    private static readonly UserTokenService _userTokenService = new UserTokenService();

    public static Task Authenticate(
        HttpContext _context, Func<Task> next, UserServices _userService)
    {
        var key = GetToken(_context.Request.Headers["Authorization"].ToString());

        var requestPath = _context.Request.Path;
        var requestMethod = _context.Request.Method;

        if (requestPath.ToString().Contains("/api/users") && requestPath.ToString() != "/api/users/all")
        {
            // for now we will allow it to pass through
            if (requestPath.ToString() == "/api/users" && requestMethod.ToString() == "POST" ||
                requestPath.ToString() == "/api/users/login" && requestMethod.ToString() == "POST"
            )
            {
                // TODO: Implement application authentication so we can 
                // ensure valid applications are creating users or users are logging in to valid applications
                return next();
            }
            // everything else requires authentication
            if (string.IsNullOrEmpty(key))
            {
                return HandleUnauthorizedRequest(_context);
            }
            else
            {
                // check if the token is valid
                var isValidToken = AuthenticateToken(key);
                // check if the user exists
                var isValidUser = AuthenticateUser(_userTokenService.GetUsernameFromToken(key),
                 _userTokenService.GetEmailFromToken(key), _userTokenService.GetIdFromToken(key),
                  _userService);

                // if the token data is bad or the user does not exist then return unauthorized
                if (!isValidUser.Result || !isValidToken)
                {
                    return HandleUnauthorizedRequest(_context);
                }
                else
                {
                    return next();
                }
            }
        }
        else
        {
            return next();
        }
    }

    private static async Task<bool> AuthenticateUser(
        string username, string email, string id, UserServices userService)
    {
        // check if the user exists by validating the claims
        var areValidClaims = await userService.ValidateUserClaimsAsync(username, email, id);

        return areValidClaims;
    }

    private static bool AuthenticateToken(string token)
    {
        // check if the token is valid
        var isValidToken = _userTokenService.IsTokenValid(token);

        return isValidToken;
    }

    private static Task HandleUnauthorizedRequest(HttpContext _context)
    {
        _context.Response.StatusCode = 401;
        // write a json message that adheres to the API response standard
        var apiError = new ApiResponse<string>
        {
            StatusCode = 401,
            Error = "Unauthorized"
        };
        return _context.Response.WriteAsJsonAsync(apiError);
    }

    public static string GetToken(string authHeader)
    {
        return authHeader.Replace("Bearer ", "");
    }
}
