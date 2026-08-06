namespace EarnTrackerApi.Exceptions;

public sealed class ConflictException(string message)
    : AppException(
        message,
        StatusCodes.Status409Conflict,
        "Conflict");
