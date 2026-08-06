namespace EarnTrackerApi.Exceptions;

public sealed class ExternalServiceException(string provider, string message)
    : AppException(
        $"{provider} request failed: {message}",
        StatusCodes.Status502BadGateway,
        "External service unavailable");
