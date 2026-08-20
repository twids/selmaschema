using System.Security.Cryptography;
using System.Text;

namespace CoParenting.Application.Services;

public static class TokenService
{
    public static string GenerateToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public static string GenerateInvitationCode()
    {
        const string alphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";
        var bytes = RandomNumberGenerator.GetBytes(8);
        var characters = bytes.Select(value => alphabet[value % alphabet.Length]).ToArray();
        return $"{new string(characters[..4])}-{new string(characters[4..])}";
    }

    public static string NormalizeInvitationCode(string code) =>
        new(code.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

    public static string HashToken(string rawToken)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
    }
}
