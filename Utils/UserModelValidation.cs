using System.Text.Json;
using WebAppAuthenticationServer.Models;
using WebAppAuthenticationServer.Services;

namespace WebAppAuthenticationServer.Utils;

public static class UserModelFieldValidation
{
    public static UserServices? _userServices;
    public static string[] ValidatableFields = new string[] { "Username", "Email", "Password" };
    // create a dictionary of validators
    public static readonly Dictionary<string, Func<object?, Task<List<DataValidationMessage<string>>>>> _validators =
     new Dictionary<string, Func<object?, Task<List<DataValidationMessage<string>>>>>
     {
        {"Username", ValidateUsername},
        {"Email", ValidateEmail},
        {"Password", ValidatePassword}
     };

    public static void SetUserServices(UserServices userServices)
    {
        _userServices = userServices;
    }

    public static void ClearUserServices()
    {
        _userServices = null;
    }
    // username
    public static async Task<List<DataValidationMessage<string>>> ValidateUsername(object? value)
    {
        List<DataValidationMessage<string>> messages = new List<DataValidationMessage<string>>();
        var validator = new Validators();

        messages.Add(validator.IsValid(value, "DoesExist"));
        messages.Add(validator.IsValid(value, "IsBetween3-20"));
        messages.Add(validator.IsValid(value, "HasNoWhiteSpace"));
        messages.Add(validator.IsValid(value, "IsAlphaNumeric"));

        // usernames must also be unique so we need to check the database
        var usernameExists = await _userServices?.UserExistsAsyncByUsername(value?.ToString()!)!;
        if (usernameExists)
        {
            messages.Add(new DataValidationMessage<string>
            {
                Message = new _Message<string>
                {
                    Message = "Usernames must be unique!",
                    Code = 400
                },
                IsValid = false
            });
        }
        return messages;
    }

    // email
    static public async Task<List<DataValidationMessage<string>>> ValidateEmail(object? value)
    {
        List<DataValidationMessage<string>> messages = new List<DataValidationMessage<string>>();
        var validator = new Validators();

        messages.Add(validator.IsValid(value, "DoesExist"));
        messages.Add(validator.IsValid(value, "IsEmail"));
        messages.Add(validator.IsValid(value, "IsBelow150"));


        var emailExists = await _userServices?.UserExistsAsyncByEmail(value?.ToString()!)!;
        if (emailExists)
        {
            messages.Add(new DataValidationMessage<string>
            {
                Message = new _Message<string>
                {
                    Message = "Emails must be unique!",
                    Code = 400
                },
                IsValid = false
            });
        }

        return messages;
    }

    // password
    static public async Task<List<DataValidationMessage<string>>> ValidatePassword(object? value)
    {
        List<DataValidationMessage<string>> messages = new List<DataValidationMessage<string>>();
        var validator = new Validators();

        messages.Add(validator.IsValid(value, "DoesExist"));
        messages.Add(validator.IsValid(value, "IsBetween8-20"));
        messages.Add(validator.IsValid(value, "HasNoWhiteSpace"));
        messages.Add(validator.IsValid(value, "HasOneNumber"));
        messages.Add(validator.IsValid(value, "HasOneSpecialCharacter"));
        messages.Add(validator.IsValid(value, "HasUpperCase"));
        messages.Add(validator.IsValid(value, "HasLowerCase"));

        await Task.Delay(001);
        return messages;

    }
}

public class UserModelValidation
{
    public async static Task<List<FieldValidationMessage>> Validate(object model, UserServices userService)
    {
        UserModelFieldValidation.SetUserServices(userService);
        List<FieldValidationMessage> messages = new List<FieldValidationMessage>();


        bool emailValidated = false;
        bool usernameValidated = false;
        bool passwordValidated = false;

        string json = JsonSerializer.Serialize(model);


        json = json.Replace("{", "");
        json = json.Replace("}", "");

        string[] jsonArr = json.Split(",");

        foreach (string item in jsonArr)
        {
            string[] keyValue = item.Split(":");

            string key = keyValue[0].Replace("\"", "");
            string value = keyValue[1].Replace("\"", "");

            if (UserModelFieldValidation.ValidatableFields.Contains(key))
            {
                FieldValidationMessage message = new FieldValidationMessage();

                message.PropertyName = key;

                if (key == "Email")
                {
                    emailValidated = true;
                }

                if (key == "Username")
                {
                    usernameValidated = true;
                }

                if (key == "Password")
                {
                    passwordValidated = true;
                }

                // get the validator for the key
                var validationStatus = await UserModelFieldValidation._validators[key](value);

                // check if we have any errors

                var hasErrors = validationStatus.Any(x => x.IsValid == false);
                var getErrors = () => validationStatus.Where(x => x.IsValid == false).Select(x => x.Message).ToList();

                if (hasErrors)
                {
                    // we need to get just the error messages from the getErrors list
                    message.ErrorMessages = getErrors();
                    message.IsValid = false;
                    messages.Add(message);
                }
                else
                {
                    message.IsValid = true;
                }
            }
        }

        // If the data doesn't adhere to the schema theres a chance the data could
        // skip validation. This explicit check ensures that the data was checked
        if (emailValidated && usernameValidated && passwordValidated)
        {
            return messages;
        }
        else
        {
            // Most likely the data doesn't adhere to the schema

            FieldValidationMessage message = new FieldValidationMessage();
            message.PropertyName = "Data Schema";
            message.IsValid = false;

            var _message = new _Message<string>();
            _message.Message = "Data schema is invalid";
            message.ErrorMessages = new List<_Message<string>> { _message };
            messages.Add(message);

            return messages;
        }
    }
}
