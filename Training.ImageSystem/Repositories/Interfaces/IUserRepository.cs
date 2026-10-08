using Training.ImageSystem.Models;
namespace Training.ImageSystem.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(
       string username,
       CancellationToken cancellationToken = default);
}
