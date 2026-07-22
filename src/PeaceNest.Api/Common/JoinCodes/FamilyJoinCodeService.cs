using System.Security.Cryptography;
using System.Text;

namespace PeaceNest.Api.Common.JoinCodes;

public sealed class FamilyJoinCodeService
{
    private const int CodeLength = 10;
    private const string CodeAlphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

    public string GenerateCode()
    {
        Span<char> characters = stackalloc char[CodeLength];
        for (var index = 0; index < characters.Length; index++)
        {
            characters[index] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];
        }

        return $"{characters[..5]}-{characters[5..]}";
    }

    public string Normalize(string code) =>
        new(code
            .Where(character => character is not '-' && !char.IsWhiteSpace(character))
            .Select(char.ToUpperInvariant)
            .ToArray());

    public bool IsValid(string code)
    {
        var normalized = Normalize(code);
        return normalized.Length == CodeLength && normalized.All(CodeAlphabet.Contains);
    }

    public string Hash(string code)
    {
        var bytes = Encoding.UTF8.GetBytes(Normalize(code));
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }
}
