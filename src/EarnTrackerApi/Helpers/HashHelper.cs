using System.Security.Cryptography;
using System.Text;

namespace EarnTrackerApi.Helpers;

public sealed class HashHelper
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 210_000;
    private const string Version = "v2";

    public required string SecretKey { get; init; }

    public static string Hash(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            AddSecretKey(value),
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return string.Join(
            '.',
            Version,
            Iterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public static bool Verify(string value, string encodedHash)
    {
        if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(encodedHash))
        {
            return false;
        }

        var parts = encodedHash.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 4 || parts[0] != Version ||
            !int.TryParse(parts[1], out var iterations) || iterations < 1)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                AddSecretKey(value),
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public static string HashToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return Convert.ToHexString(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(HashHelperSettings.SecretKey),
            Encoding.UTF8.GetBytes(token)));
    }

    private static string AddSecretKey(string value)
    {
        return string.Concat(value, "\0", HashHelperSettings.SecretKey);
    }
}

public static class HashHelperSettings
{
    public static string SecretKey { get; private set; } = string.Empty;

    public static void Configure(IConfiguration configuration)
    {
        var settings = configuration
            .GetSection("HashHelper")
            .Get<HashHelper>();
        var secretKey = settings?.SecretKey;

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new ArgumentNullException(
                nameof(settings.SecretKey),
                "HashHelper:SecretKey cannot be null or empty.");
        }

        if (Encoding.UTF8.GetByteCount(secretKey) < 32)
        {
            throw new InvalidOperationException(
                "HashHelper:SecretKey must be at least 32 bytes long.");
        }

        SecretKey = secretKey;
    }
}
