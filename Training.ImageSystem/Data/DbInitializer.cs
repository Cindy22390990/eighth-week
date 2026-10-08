using Microsoft.EntityFrameworkCore;
using Training.ImageSystem.Models;
using Training.ImageSystem.Security.Interfaces;

namespace Training.ImageSystem.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        AppDbContext context,
        IPasswordService passwordService)
    {
        //await context.Database.MigrateAsync();


        if (await context.Users.AnyAsync())
        {
            return;
        }


        var admin = new User
        {
            Username = "admin",
            Role = "Admin",
            CreatedAtUtc = DateTime.UtcNow
        };


        admin.PasswordHash =
            passwordService.HashPassword(
                admin,
                "123456"
            );


        context.Users.Add(admin);


        await context.SaveChangesAsync();
    }
}