using System.Net.Mail;
using System.Security.Cryptography;

namespace FridgeApi.Models;

// A person who can log in (one row in the Users table). The password is never stored, only its hash.
public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedOn { get; set; }
}

// What the UI sends to sign up or log in.
public record AuthRequest(string? Email, string? Password)
{
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 128;

    // Emails are stored trimmed and in lower case, so "Ann@X.com" and "ann@x.com" are the same account.
    public string NormalizedEmail => (Email ?? "").Trim().ToLowerInvariant();

    public Dictionary<string, string[]> Validate()
    {
        var errors = new Dictionary<string, string[]>();
        var email = NormalizedEmail;
        if (email.Length == 0)
            errors["email"] = ["Email is required."];
        else if (email.Length > 256 || !MailAddress.TryCreate(email, out var parsed) || parsed.Address != email)
            errors["email"] = ["Enter a valid email address."];
        if (string.IsNullOrEmpty(Password))
            errors["password"] = ["Password is required."];
        else if (Password.Length < MinPasswordLength)
            errors["password"] = [$"Password must be at least {MinPasswordLength} characters."];
        else if (Password.Length > MaxPasswordLength)
            errors["password"] = [$"Password must be {MaxPasswordLength} characters or fewer."];
        return errors;
    }
}

// What the API sends back about the logged-in user.
public record UserResponse(string Email);

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
