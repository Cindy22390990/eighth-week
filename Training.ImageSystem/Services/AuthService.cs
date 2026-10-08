using Training.ImageSystem.Models.DTOs;
using Training.ImageSystem.Repositories.Interfaces;
using Training.ImageSystem.Security.Interfaces;
using Training.ImageSystem.Services.Interfaces;

namespace Training.ImageSystem.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    
    public AuthService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null)
        {
            return null;
        }
        var passwordValid =
            _passwordService.VerifyPassword(
                user,
                user.PasswordHash,
                request.Password);
        if (!passwordValid) 
        { 
            return null;
        }
        var token =
            _jwtService.GenerateToken(user);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = _jwtService.GetExpirationTime()
        };
        
    }
}
