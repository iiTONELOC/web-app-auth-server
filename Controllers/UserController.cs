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

    // GET ALL USERS - returns unauthorized
    [HttpGet]
    [Route("all")]
    public async Task<ActionResult<List<User>>> Get() =>
    await _userServices.GetAsync();


    // GET USER by id, username, email
    [HttpGet(Name = "GetUser")]
    public async Task<ApiResponse<object>> GetAsync([FromBody] User user)
    {
        // holds the user data to be returned
        User? _user;

        var hasId = user.Id != null;
        var hasUsername = user.Username != null;
        var hasEmail = user.Email != null;

        // fetch the data but don't return it yet
        if (hasId)
        {
            _user = await _userServices.GetAsync(user?.Id!);
        }
        else if (hasUsername)
        {
            _user = await _userServices.GetAsyncByUsername(user?.Username!);
        }
        else if (hasEmail)
        {
            _user = await _userServices.GetAsyncByEmail(user?.Email!);
        }
        else
        {
            _user = null;
        }

        // create the response object
        var response = new ApiResponse<object> { };

        // see if the user is authorized to view the data
        var isAuth = CanViewOrMutate(_user?.Id!);

        // user doesn't exist
        if (_user == null)
        {
            response.Data = null;
            response.StatusCode = 404;
            response.Error = Responses.NotFound;
        }
        else
        {
            if (isAuth)
            {
                response.Data = _user;
                response.StatusCode = 200;
            }
            else
            {
                response.Data = null;
                response.StatusCode = 401;
                response.Error = Responses.Unauthorized;
            }
        }

        Response.StatusCode = response.StatusCode;
        return response;
    }


    // CREATE A USER
    [HttpPost(Name = "CreateUser")]
    public async Task<ApiResponse<object>> CreateAsync([FromBody] NonHashedUserInfo user)
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
            string token = JWTService.GenerateToken(_user.Username!, _user.Email!, _user.Id!);

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


    // EDIT USER 
    [HttpPut(Name = "UpdateUser")]
    public async Task<ApiResponse<object>> UpdateAsync([FromBody] NonHashedUserInfo userData)
    {
        // we need to get the id of the data to edit to see if the user can edit it
        var userToEditData = await _userServices.GetAsyncByUsername(userData?.Id!);
        var canEdit = CanViewOrMutate(userToEditData?.Id!);

        // create the response object
        var response = new ApiResponse<object> { };


        if (canEdit)
        {
            // store the data in easier to access variables
            var id = userData?.Id;
            var username = userData?.Username;
            var email = userData?.Email;
            var password = userData?.Password;

            // must have the id to mutate the user
            if (id == null)
            {
                response.StatusCode = 400;
                response.Data = null;
                response.Error = new _Message<string>
                {
                    Message = "Data received was in an unexpected format",
                    Code = 400
                };
            }
            else
            {
                // check the properties that are present - except for passwords
                var hasUsername = username != null;
                var hasEmail = email != null;

                var resetUsername = !hasUsername;
                var resetEmail = !hasEmail;


                // we dont reset passwords this way so just set it to something
                // to pass validation
                userData.Password = "passwordP1?";

                if (resetUsername)
                    userData.Username = "username";
                if (resetEmail)
                    userData.Email = "email@test.com";


                // validate the data
                List<FieldValidationMessage> messages = await UserModelValidation
                .Validate(userData, _userServices);

                // reset the data to null if we are not actually updating it
                if (resetUsername)
                    userData.Username = null;
                if (resetEmail)
                    userData.Email = null;

                // passwords are not reset this way
                userData.Password = null;

                // if there are no messages then we can update the user
                if (messages.Count == 0)
                {
                    // get the user
                    var user = await _userServices.GetAsync(id);
                    if (user == null)
                    {
                        response.StatusCode = 404;
                        response.Data = null;
                        response.Error = Responses.NotFound;
                    }
                    else
                    {
                        // update the user's properties
                        if (hasUsername)
                        {
                            user.Username = username!;
                        }
                        if (hasEmail)
                        {
                            user.Email = email!;
                        }

                        await _userServices.UpdateAsync(id, user);

                        // generate a new token for the user to reflect the changes
                        var JWTService = new UserTokenService();
                        var updatedUserData = await _userServices.GetAsync(id);

                        string token = JWTService
                        .GenerateToken(
                            updatedUserData?.Username!, updatedUserData?.Email!, updatedUserData?.Id!);

                        response.StatusCode = 200;
                        response.Data = token;
                    }
                }
                else
                {
                    response.StatusCode = 400;
                    response.Data = null;
                    // send the validation messages back to the user
                    response.Error = messages;
                }
            }
        }
        else
        {
            response.Data = null;
            response.StatusCode = 401;
            response.Error = Responses.Unauthorized;
        }

        Response.StatusCode = response.StatusCode;
        return response;
    }


    // DELETE USER
    [HttpDelete("{id:length(24)}", Name = "RemoveUser")]
    public async Task<ApiResponse<object>> RemoveAsync(string id)
    {
        // see if the user is authorized to delete the user
        var _user = await _userServices.GetAsync(id);

        var canDelete = CanViewOrMutate(_user?.Id!);

        // create the response object
        var response = new ApiResponse<object> { };

        if (canDelete)
        {
            // get the user

            await _userServices.RemoveAsync(_user!);
            // the user was deleted successfully
            response.StatusCode = 200;
            response.Data = null;
        }
        else
        {
            response.Data = null;
            response.StatusCode = 401;
            response.Error = Responses.Unauthorized;
        }

        Response.StatusCode = response.StatusCode;
        return response;
    }


    // LOGIN
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
            response.Error = Responses.Unauthorized;

        }
        else
        {
            // the user was validated successfully
            string token = JWTService.GenerateToken(_user?.Username!, _user?.Email!, _user?.Id!);
            response.Data = token;
            response.StatusCode = 200;
        }

        Response.StatusCode = response.StatusCode;
        return response;
    }


    // ENSURES THE USER IS AUTHORIZED TO VIEW OR MUTATE THE REQUESTED USER DATA

    /// <summary>
    /// Verifies that the user is requesting to view or mutate their own data 
    /// </summary>
    private bool CanViewOrMutate(string idToCheckAgainst)
    {
        // operate on zero trust, validate the token again
        var token = UserAuthService.GetToken(Request.Headers["Authorization"].ToString());
        var JWTService = new UserTokenService();
        var isValidToken = JWTService.IsTokenValid(token);

        // if the token is valid
        if (isValidToken)
        {
            // get the username from the token
            var username = JWTService.GetUsernameFromToken(token);

            // get the email from the token
            var email = JWTService.GetEmailFromToken(token);

            var _id = JWTService.GetIdFromToken(token);

            // the username and email matches the token
            var userExists = _userServices.ValidateUserClaimsAsync(username, email, _id);


            if (userExists.Result == true)
            {
                // check the id
                if (_id == idToCheckAgainst)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
