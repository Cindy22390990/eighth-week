using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Training.ImageSystem.Models;
using Training.ImageSystem.Services.Interfaces;

namespace Training.ImageSystem.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

        var credentials =
            new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            // 使用者 ID
            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()
            ),

            // 使用者名稱
            new(
                ClaimTypes.Name,
                user.Username
            ),

            // 角色
            new(
                ClaimTypes.Role,
                user.Role
            )
        };

        var expires = GetExpirationTime();

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public DateTime GetExpirationTime()
    {
        var jwtSettings = _configuration.GetSection("Jwt");


        return DateTime.UtcNow.AddMinutes(
            Convert.ToDouble(jwtSettings["ExpireMinutes"])
        );
    }
}