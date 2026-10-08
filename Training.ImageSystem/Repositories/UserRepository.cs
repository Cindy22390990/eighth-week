using Microsoft.EntityFrameworkCore;
using Training.ImageSystem.Data;
using Training.ImageSystem.Models;
using Training.ImageSystem.Repositories.Interfaces;

namespace Training.ImageSystem.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context)
    {  
        _context = context; 
    }
    public async Task<User?> GetByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                user => user.Username == username,
                cancellationToken);
    }
}
