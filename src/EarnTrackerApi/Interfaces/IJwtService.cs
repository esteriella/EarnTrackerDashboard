using EarnTrackerApi.Dtos.AuthDto;
using EarnTrackerApi.Models;

namespace EarnTrackerApi.Interfaces;

public interface IJwtService
{
    AuthTokenResponse CreateToken(User user);
}
