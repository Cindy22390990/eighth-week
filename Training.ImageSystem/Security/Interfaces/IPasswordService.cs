using Training.ImageSystem.Models;

namespace Training.ImageSystem.Security.Interfaces;

public interface IPasswordService
{
    string HashPassword(User user, string password);

    bool VerifyPassword(
        User user,
        string hashedPassword,
        string inputPassword);
}
