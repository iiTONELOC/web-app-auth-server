using System.ComponentModel.DataAnnotations;

namespace WebAppAuthenticationServer.Utils;

static public class InputValidation
{
    /// <summary>
    /// Validates input against a regular expression
    /// </summary>
    public static bool _validator(object? value, string regex)
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

    /// <summary>
    /// Validates that a string contains at least one uppercase letter
    /// </summary>
    public class HasUpperCase : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return _validator(value, "^[A-Z]*$");
        }
    }

    /// <summary>
    /// Validates that a string contains at least one lowercase letter
    /// </summary>
    public class HasLowerCase : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return _validator(value, "^[a-z]*$");
        }
    }

    /// <summary>
    /// Validates that a string contains at least one number
    /// </summary>
    public class HasOneNumber : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return _validator(value, "^[0-9]*$");
        }
    }

    /// <summary>
    /// Validates that a string contains at least one special character
    /// </summary>
    public class HasOneSpecialCharacter : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return _validator(value, "^[!@#$%^&()-_+=/|{}]*$");
        }
    }

    /// <summary>
    /// Validates that a string contains no whitespace
    /// </summary>
    public class NoWhiteSpace : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            return _validator(value, "^[^\\s]*$");
        }
    }
}
