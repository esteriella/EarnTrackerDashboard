namespace EarnTrackerApi.Exceptions;

public sealed class NotFoundException(string resource)
    : AppException(
        $"{resource} was not found.",
        StatusCodes.Status404NotFound,
        "Resource not found");
