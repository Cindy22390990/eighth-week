using Training.ImageSystem.Models.DTOs;
namespace Training.ImageSystem.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}
