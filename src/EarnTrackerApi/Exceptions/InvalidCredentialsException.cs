namespace EarnTrackerApi.Exceptions;

public sealed class InvalidCredentialsException()
    : AppException(
        "Email or password is incorrect.",
        StatusCodes.Status401Unauthorized,
        "Login failed");
