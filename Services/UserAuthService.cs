using WebAppAuthenticationServer.Models;

namespace WebAppAuthenticationServer.Services;

public static class UserAuthService
{
    private static readonly UserTokenService _userTokenService = new UserTokenService();

    public static Task Authenticate(HttpContext _context, Func<Task> next, UserServices _userService)
    {
        var key = _context.Request.Headers["Authorization"]
             .ToString()
             .Replace("Bearer ", "");

        var requestPath = _context.Request.Path;
        var requestMethod = _context.Request.Method;

        // for now we will allow it to pass through
        if (requestPath.ToString() == "/api/users" && requestMethod.ToString() == "POST" ||
            requestPath.ToString() == "/api/users/login" && requestMethod.ToString() == "POST"
        )
        {
            // TODO: Implement application authentication so we can 
            // ensure valid applications are creating users or users are logging in to valid applications
            return next();
        }
        if (requestPath.ToString() == "/api/users" && requestMethod.ToString() == "GET")
        {
            // TODO: Implement application authentication to retrieve this data
            // users cannot view a list of all users
            // return unauthorized
            return HandleUnauthorizedRequest(_context);
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
             _userTokenService.GetEmailFromToken(key), _userService);

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

    private static async Task<bool> AuthenticateUser(string username, string email, UserServices userService)
    {
        // check if the user exists
        var isValidUser = await userService.UserExistsAsync(username, email);

        return isValidUser;
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
}
