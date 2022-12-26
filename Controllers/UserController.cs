using Microsoft.AspNetCore.Mvc;
using WebAppAuthenticationServer.Utils;
using WebAppAuthenticationServer.Models;
using WebAppAuthenticationServer.Services;

namespace WebAppAuthenticationServer.Controllers;


[ApiController]
[Route("api/[controller]")]
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

    [HttpGet(Name = "GetUser")]
    public async Task<ActionResult<User>> GetAsync([FromBody] string id)
    {
        var user = await _userServices.GetAsync(id);

        if (user == null)
        {
            return BadRequest();
        }

        return user;
    }


    [HttpGet(Name = "GetUserByUsername")]
    public async Task<ActionResult<User>> GetAsyncByUsername([FromBody] string username)
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

            response.Data = _user;
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

    [HttpDelete(Name = "RemoveUser")]
    public async Task<IActionResult> RemoveAsync([FromBody] string id)
    {
        var user = await _userServices.GetAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        await _userServices.RemoveAsync(user);

        return NoContent();
    }
}