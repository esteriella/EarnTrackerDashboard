using Npgsql;

namespace EarnTrackerApi.Helpers;

public static class PostgresConnectionString
{
    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                "A PostgreSQL connection string is required.");
        }

        if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException(
                "DATABASE_URL is not a valid PostgreSQL URL.");
        }

        var credentials = uri.UserInfo.Split(':', 2);
        if (credentials.Length != 2)
        {
            throw new InvalidOperationException(
                "DATABASE_URL must contain a username and password.");
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = Uri.UnescapeDataString(credentials[1])
        };

        foreach (var parameter in uri.Query.TrimStart('?').Split(
            '&',
            StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = parameter.Split('=', 2);
            var key = Uri.UnescapeDataString(parts[0]);
            var parameterValue = parts.Length == 2
                ? Uri.UnescapeDataString(parts[1])
                : string.Empty;

            if (key.Equals("sslmode", StringComparison.OrdinalIgnoreCase))
            {
                builder.SslMode = parameterValue.Equals(
                    "require",
                    StringComparison.OrdinalIgnoreCase)
                        ? SslMode.Require
                        : builder.SslMode;
            }
        }

        return builder.ConnectionString;
    }
}
