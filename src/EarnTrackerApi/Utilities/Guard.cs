namespace EarnTrackerApi.Utilities;

public static class Guard
{
    public static string Required(string? value, string parameterName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A value is required.", parameterName)
            : value.Trim();
    }
}
