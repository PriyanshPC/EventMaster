using System.Text.RegularExpressions;

namespace EventMaster.Api.Security;
/// <summary>
/// Provides static methods for validating user input related to authentication, such as full name, phone number, email, and password strength.
/// This class uses regular expressions to enforce specific formats and rules for each type of input, ensuring that data meets the expected criteria before being processed further in the authentication workflow.
/// The validation rules are as follows:
/// - Full Name: Must consist of exactly two words (first and last name) separated by
/// a single space, with optional apostrophes or hyphens within each word (e.g., "O'Neil", "Anne-Marie").
/// - Phone Number: Must be a 10-digit number starting with digits 2-9
/// - Email: Must follow a standard email format
/// </summary>
public static class AuthValidation
{
    // Exactly two words separated by a single space. No leading/trailing spaces.
    // Allows letters plus optional apostrophe/hyphen inside words: O'Neil, Anne-Marie
    private static readonly Regex FullNameRegex =
        new(@"^[A-Za-z]+(?:['-][A-Za-z]+)? [A-Za-z]+(?:['-][A-Za-z]+)?$",
            RegexOptions.Compiled);

    private static readonly Regex PhoneRegex =
        new(@"^[2-9]\d{9}$", RegexOptions.Compiled);

    // >=8, 1 uppercase, 1 digit, 1 special (non-alphanumeric)
    private static readonly Regex StrongPasswordRegex =
        new(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$", RegexOptions.Compiled);

    private static readonly Regex EmailRegex =
        new(@"^([A-Za-z0-9._-]+@[a-zA-Z0-9.-]+\.[a-z]{2,}$)", RegexOptions.Compiled);


    public static bool IsValidFullName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return FullNameRegex.IsMatch(name.Trim());
    }

    public static bool IsValidPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;
        return PhoneRegex.IsMatch(phone.Trim());
    }

    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        return EmailRegex.IsMatch(email);
    }

    public static bool IsStrongPassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
        return StrongPasswordRegex.IsMatch(password);
    }
}
