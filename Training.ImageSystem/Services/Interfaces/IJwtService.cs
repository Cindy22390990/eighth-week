using Training.ImageSystem.Models;
namespace Training.ImageSystem.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    DateTime GetExpirationTime();
}
