namespace EarnTrackerApi.Exceptions;

public class AppException(
    string message,
    int statusCode = StatusCodes.Status400BadRequest,
    string title = "Request failed") : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Title { get; } = title;
}
