using WebAppAuthenticationServer.Models;


namespace WebAppAuthenticationServer.Utils;

public class Validators
{

    protected Dictionary<string, (Func<object?, bool>, string)> _validators =
     new Dictionary<string, (Func<object?, bool>, string)>
    {
        { "IsEmail", (IsEmail, "Invalid Email Address!") },
        { "DoesExist", (DoesExist, "This field is required!") },
        { "HasOneNumber", (HasOneNumber, "A number is required!") },
        { "HasNoWhiteSpace", (HasNoWhiteSpace, "No whitespace is allowed!") },
        { "HasLowerCase", (HasLowerCase, "A lowercase letter is required!") },
        { "HasUpperCase", (HasUpperCase, "An uppercase letter is required!") },
        {"IsAlphaNumeric", (IsAlphaNumeric, "This field must be alphanumeric!")},
        {"IsBelow150", (IsBelow150, "This field must have less than 150 characters!")},
        { "HasOneSpecialCharacter", (HasOneSpecialCharacter, "A special character is required!") },
        {"IsBetween3-20", (IsBetween3and20, "This field must be between 8 and 20 characters long!") },
        { "IsBetween8-20", (IsBetween8and20, "This field must be between 8 and 20 characters long!") },
    };

    public DataValidationMessage<string> IsValid(object? value, string validator)
    {
        DataValidationMessage<string> message = new DataValidationMessage<string>();


        if (value == null)
        {
            message.Message = new _Message<string>();
            message.Message.Code = 400;
            message.Message.Message = "This field is required!";
            message.IsValid = false;
        }
        else
        {
            message.Message = new _Message<string>();
            message.IsValid = _validators[validator].Item1(value);
            message.Message.Code = message.IsValid ? 200 : 400;
            message.Message.Message = message.IsValid ? "Valid!" : GetErrorMessage(validator);

        }
        return message;
    }

    public string GetErrorMessage(string validator)
    {
        return _validators[validator].Item2;
    }

    // actual validator function that returns a bool
    static protected bool _validator(object? value, string regex)
    {
        if (value == null)
        {
            return false;
        }
        else
        {
            return System.Text.RegularExpressions
            .Regex.IsMatch(value.ToString()!, regex);
        }
    }

    static protected bool HasUpperCase(object? value)
    {
        return _validator(value, @"[A-Z]");
    }

    static protected bool IsAlphaNumeric(object? value)
    {
        return _validator(value, @"^[a-zA-Z0-9]*$");
    }

    static protected bool HasLowerCase(object? value)
    {
        return _validator(value, @"[a-z]");
    }

    static protected bool HasOneNumber(object? value)
    {
        return _validator(value, @"[0-9]");
    }

    static protected bool HasOneSpecialCharacter(object? value)
    {
        return _validator(value, @"[!@#$%^&*)}[|/.+=}(\]\?_~`\-;:]");
    }

    static protected bool HasNoWhiteSpace(object? value)
    {
        return _validator(value, @"[^\\s]");
    }

    static protected bool DoesExist(object? value)
    {
        return value != null && value?.ToString()?.Length > 0;
    }

    static protected bool _HasRange(object? value, int min, int max)
    {
        return value?.ToString()?.Length >= min && value?.ToString()?.Length <= max;
    }

    static protected bool IsBelow150(object? value)
    {
        return _HasRange(value, 1, 150);
    }

    static protected bool IsBetween8and20(object? value)
    {
        return _HasRange(value, 8, 20);
    }

    static protected bool IsBetween3and20(object? value)
    {
        return _HasRange(value, 3, 20);
    }

    static protected bool IsEmail(object? value)
    {
        var email = value?.ToString() ?? string.Empty;
        var trimmedEmail = email.Trim();

        if (trimmedEmail.EndsWith("."))
        {
            return false; // suggested by @TK-421
        }
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == trimmedEmail;
        }
        catch
        {
            return false;
        }
    }
}
