using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace FridgeApi.Models;

// A person who can log in (one row in the Users table). The password is never stored, only its hash.
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedOn { get; set; }
}

// Shared rules for sign-up and login fields.
public static partial class UserRules
{
    public const int MaxNameLength = 100;
    public const int MaxPhoneLength = 20;
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 128;

    // Emails are stored trimmed and in lower case, so "Ann@X.com" and "ann@x.com" are the same account.
    public static string NormalizeEmail(string? email) => (email ?? "").Trim().ToLowerInvariant();

    public static bool IsValidEmail(string email) =>
        email.Length <= 256 && MailAddress.TryCreate(email, out var parsed) && parsed.Address == email;

    // Digits, spaces, dashes, brackets and an optional leading +, with 7 to 15 digits in total.
    public static bool IsValidPhone(string phone) =>
        PhonePattern().IsMatch(phone) && phone.Count(char.IsAsciiDigit) is >= 7 and <= 15;

    [GeneratedRegex(@"^\+?[0-9 ()\-]+$")]
    private static partial Regex PhonePattern();
}

// What the UI sends to create an account.
public record RegisterRequest(string? Name, string? Phone, string? Email, string? Password)
{
    public string NormalizedEmail => UserRules.NormalizeEmail(Email);

    public Dictionary<string, string[]> Validate()
    {
        var errors = new Dictionary<string, string[]>();

        var name = Name?.Trim() ?? "";
        if (name.Length == 0)
            errors["name"] = ["Name is required."];
        else if (name.Length > UserRules.MaxNameLength)
            errors["name"] = [$"Name must be {UserRules.MaxNameLength} characters or fewer."];

        var phone = Phone?.Trim() ?? "";
        if (phone.Length == 0)
            errors["phone"] = ["Phone number is required."];
        else if (phone.Length > UserRules.MaxPhoneLength || !UserRules.IsValidPhone(phone))
            errors["phone"] = ["Enter a valid phone number (7 to 15 digits; +, spaces, dashes and brackets allowed)."];

        var email = NormalizedEmail;
        if (email.Length == 0)
            errors["email"] = ["Email is required."];
        else if (!UserRules.IsValidEmail(email))
            errors["email"] = ["Enter a valid email address."];

        if (string.IsNullOrEmpty(Password))
            errors["password"] = ["Password is required."];
        else if (Password.Length < UserRules.MinPasswordLength)
            errors["password"] = [$"Password must be at least {UserRules.MinPasswordLength} characters."];
        else if (Password.Length > UserRules.MaxPasswordLength)
            errors["password"] = [$"Password must be {UserRules.MaxPasswordLength} characters or fewer."];

        return errors;
    }
}

// What the UI sends to log in.
public record LoginRequest(string? Email, string? Password)
{
    public string NormalizedEmail => UserRules.NormalizeEmail(Email);
}

// What the API sends back about the logged-in user.
public record UserResponse(string Name, string Email);

// Turns a password into a salted PBKDF2 hash ("iterations.salt.hash") and checks a password against it.
public static class PasswordHashing
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public static bool Verify(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
            return false;
        var salt = Convert.FromBase64String(parts[1]);
        var expected = Convert.FromBase64String(parts[2]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
