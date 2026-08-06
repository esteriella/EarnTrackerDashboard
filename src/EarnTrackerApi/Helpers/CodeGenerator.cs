using System.Security.Cryptography;

namespace EarnTrackerApi.Helpers;

public static class CodeGenerator
{
    public static string GenerateNumericCode(int length = 6)
    {
        if (length is < 4 or > 12)
        {
            throw new ArgumentOutOfRangeException(
                nameof(length),
                "Code length must be between 4 and 12 digits.");
        }

        Span<char> code = stackalloc char[length];
        for (var index = 0; index < code.Length; index++)
        {
            code[index] = (char)('0' + RandomNumberGenerator.GetInt32(10));
        }

        return new string(code);
    }

    public static string GenerateSecureToken(int byteLength = 32)
    {
        if (byteLength is < 16 or > 128)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteLength),
                "Token size must be between 16 and 128 bytes.");
        }

        return Convert.ToHexString(RandomNumberGenerator.GetBytes(byteLength));
    }
}
