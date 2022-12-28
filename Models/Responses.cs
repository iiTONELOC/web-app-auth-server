
namespace WebAppAuthenticationServer.Models;

public static class Responses
{
    public static _Message<string> BadRequest = new _Message<string>
    {
        Code = 400,
        Message = "Bad Request"
    };

    public static _Message<string> Unauthorized = new _Message<string>
    {
        Code = 401,
        Message = "Unauthorized"
    };

    public static _Message<string> NotFound = new _Message<string>
    {
        Code = 404,
        Message = "Not Found"
    };

    public static _Message<string> InternalServerError = new _Message<string>
    {
        Code = 500,
        Message = "Internal Server Error"
    };
}