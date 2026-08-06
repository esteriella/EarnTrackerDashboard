using EarnTrackerApi.Dtos.AuthDto;
using EarnTrackerApi.Exceptions;
using EarnTrackerApi.Extensions;
using EarnTrackerApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EarnTrackerApi.Controllers;

[ApiController]
[Authorize]
[Route("api/auth")]
public sealed class AuthController(
    IAuthService authService,
    IUnitOfWork unitOfWork) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponseDto>> Register(
        RegisterDto request,
        CancellationToken cancellationToken)
    {
        var response = await authService.RegisterAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(
        LoginDto request,
        CancellationToken cancellationToken)
    {
        return Ok(await authService.LoginAsync(request, cancellationToken));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponseDto>> Refresh(
        RefreshTokenDto request,
        CancellationToken cancellationToken)
    {
        return Ok(await authService.RefreshAsync(request, cancellationToken));
    }

    [HttpGet("me")]
    public async Task<ActionResult<AuthUserResponse>> GetCurrentUser(
        CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Auth.GetByIdAsync(
            User.GetUserId(),
            cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User");
        }

        return Ok(new AuthUserResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            user.CreatedAt));
    }
}
