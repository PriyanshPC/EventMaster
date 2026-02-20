using System.Text.RegularExpressions;

namespace EventMaster.Api.Security;

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
