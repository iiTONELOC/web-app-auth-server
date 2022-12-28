using WebAppAuthenticationServer.Models;

namespace WebAppAuthenticationServer.Services;

// Protects any /api/webapp endpoint
public static class WebAppAuthService
{
    private static readonly string? MasterKey = Environment.GetEnvironmentVariable("MasterKey");
    private static readonly string? ApprovedKeys = Environment.GetEnvironmentVariable("ApprovedKeyList");

    public static Task AuthenticateApplication(HttpContext _context, Func<Task> next)
    {

        var key = GetToken(_context.Request.Headers["WebApp_Authorization"].ToString());

        var requestPath = _context.Request.Path;

        // if the request is not for the all users endpoint then check if the key is approved
        if (!requestPath.ToString().Contains("/api/users/all"))
        {
            var approved = GetApprovedKeys();
            var isInApproved = approved?.Contains(key) ?? false;


            if (key == MasterKey || isInApproved)
            {
                return next();
            }
            else
            {
                return HandleUnauthorizedRequest(_context);
            }
        }
        else
        {
            // only the master key can access this endpoint
            if (key == MasterKey)
            {
                return next();
            }
            else
            {
                return HandleUnauthorizedRequest(_context);
            }
        }
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


    public static string GetToken(string webAppAuthHeader)
    {
        return webAppAuthHeader.Replace("Bearer ", "");
    }


    private static string[]? GetApprovedKeys()
    {
        var delimiter = ",";

        var hasDelim = ApprovedKeys?.Contains(delimiter);

        if (hasDelim == true)
        {
            var keys = ApprovedKeys?.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            return keys;
        }
        else
        {
            var keys = new string[1];
            keys[0] = ApprovedKeys!;

            if (keys[0] == null)
            {
                return null;
            }
            else
            {
                return keys;
            }
        }
    }
}