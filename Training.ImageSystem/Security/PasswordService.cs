using Microsoft.AspNetCore.Identity;
using Training.ImageSystem.Models;
using Training.ImageSystem.Security.Interfaces;
namespace Training.ImageSystem.Security;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _passwordHasher = new();
    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }
    public bool VerifyPassword(
        User user,
        string hashedPassword,
        string inputPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(
            user,
            hashedPassword,
            inputPassword);

        return result == PasswordVerificationResult.Success
            || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
