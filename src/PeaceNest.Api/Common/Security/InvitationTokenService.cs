using System.Security.Cryptography;
using System.Text;

namespace PeaceNest.Api.Common.Security;

public sealed class InvitationTokenService
{
    private const int TokenByteLength = 32;
    private const int CodeLength = 10;
    private const string CodeAlphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

    public string GenerateToken()
    {
        Span<byte> bytes = stackalloc byte[TokenByteLength];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncode(bytes);
    }

    public string HashToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public string GenerateCode()
    {
        Span<char> characters = stackalloc char[CodeLength];
        for (var index = 0; index < characters.Length; index++)
        {
            characters[index] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];
        }

        return $"{characters[..5]}-{characters[5..]}";
    }

    public string NormalizeCode(string code) =>
        new(code
            .Where(character => character is not '-' && !char.IsWhiteSpace(character))
            .Select(char.ToUpperInvariant)
            .ToArray());

    public string HashCode(string code) => HashToken(NormalizeCode(code));

    private static string Base64UrlEncode(ReadOnlySpan<byte> bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
