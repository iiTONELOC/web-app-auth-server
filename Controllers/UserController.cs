using Microsoft.AspNetCore.Mvc;
using WebAppAuthenticationServer.Utils;
using WebAppAuthenticationServer.Models;
using WebAppAuthenticationServer.Services;

namespace WebAppAuthenticationServer.Controllers;


[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly UserServices _userServices;

    public UserController(UserServices userServices)
    {
        _userServices = userServices;
    }

    [HttpGet]
    public async Task<ActionResult<List<User>>> Get() =>
    await _userServices.GetAsync();

    [HttpGet("{id:length(24)}", Name = "GetUser")]
    public async Task<ActionResult<User>> GetAsync(string id)
    {
        var user = await _userServices.GetAsync(id);

        if (user == null)
        {
            return BadRequest();
        }

        return user;
    }


    [HttpGet("{username}", Name = "GetUserByUsername")]
    public async Task<ActionResult<User>> GetAsyncByUsername(string username)
    {
        var user = await _userServices.GetAsyncByUsername(username);

        if (user == null)
        {
            return BadRequest();
        }

        return user;
    }


    [HttpPost(Name = "CreateUser")]
    public async Task<ApiResponse<object>> CreateAsync([FromBody] CreateUserInfo user)
    {
        // for each property of the user object we need to run validation
        List<FieldValidationMessage> messages = await UserModelValidation
        .Validate(user, _userServices);

        // create the response object
        var response = new ApiResponse<object> { };
        // create the JWT service
        var JWTService = new UserTokenService();

        // if there are no messages then we can create the user
        if (messages.Count == 0)
        {
            User _user = new User
            {
                Username = user.Username,
                Email = user.Email,
                Password = user.Password
            };

            await _userServices.CreateAsync(_user);

            // the user was created successfully
            // create a token for the user and return it
            string token = JWTService.GenerateToken(_user.Username!, _user.Email!);

            response.Data = token;
            response.StatusCode = 201;
        }
        else
        {
            response.Data = null;
            response.StatusCode = 400;
            response.Error = messages;
        }

        Response.StatusCode = response.StatusCode;
        return response;
    }



    [HttpPut(Name = "UpdateUser")]
    public async Task<IActionResult> UpdateAsync([FromBody] User userData)
    {
        var id = userData.Id;
        var user = await _userServices.GetAsync(userData.Id!);
        // remove the id from the user data object

        if (user == null)
        {
            return NotFound();
        }

        await _userServices.UpdateAsync(id!, userData);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}", Name = "RemoveUser")]
    public async Task<IActionResult> RemoveAsync(string id)
    {
        var user = await _userServices.GetAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        await _userServices.RemoveAsync(user);

        return NoContent();
    }

    [HttpPost]
    [Route("login")]
    public async Task<ApiResponse<_Message<string>>> LoginAsync([FromBody] LoginUserInfo user)
    {
        // create the response _Message<string>
        var response = new ApiResponse<_Message<string>> { };
        // create the JWT service
        var JWTService = new UserTokenService();
        // create the unauthorized message


        // check if the user exists
        var _user = await _userServices.GetAsyncByUsername(user.Username!);

        // if the user doesn't exist or the password is incorrect
        if (_user == null
        || !PassHash.VerifyPassword(user.Password!, _user.Password!, _user.PassSalt!))
        {
            response.Data = null;
            response.StatusCode = 401;
            response.Error = new _Message<string>
            {
                Code = 401,
                Message = "Invalid credentials"
            };

        }
        else
        {
            // the user was validated successfully
            string token = JWTService.GenerateToken(_user?.Username!, _user?.Email!);
            response.Data = token;
            response.StatusCode = 200;
        }

        Response.StatusCode = response.StatusCode;
        return response;
    }

}
