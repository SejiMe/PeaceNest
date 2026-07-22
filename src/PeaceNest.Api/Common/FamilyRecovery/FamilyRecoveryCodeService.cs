using System.Security.Cryptography;
using System.Text;

namespace PeaceNest.Api.Common.FamilyRecovery;

public sealed class FamilyRecoveryCodeService
{
    private const int CodeLength = 20;
    private const string CodeAlphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

    public string GenerateCode()
    {
        Span<char> characters = stackalloc char[CodeLength];
        for (var index = 0; index < characters.Length; index++)
        {
            characters[index] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];
        }

        return string.Create(
            CodeLength + 3,
            characters.ToArray(),
            static (destination, source) =>
            {
                source.AsSpan(0, 5).CopyTo(destination);
                destination[5] = '-';
                source.AsSpan(5, 5).CopyTo(destination[6..]);
                destination[11] = '-';
                source.AsSpan(10, 5).CopyTo(destination[12..]);
                destination[17] = '-';
                source.AsSpan(15, 5).CopyTo(destination[18..]);
            });
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
