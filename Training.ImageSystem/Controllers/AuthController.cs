using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.ImageSystem.Models.DTOs;
using Training.ImageSystem.Services.Interfaces;

namespace Training.ImageSystem.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;


    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        LoginRequest request)
    {
        var result =
            await _authService.LoginAsync(request);


        if (result == null)
        {
            return Unauthorized(new
            {
                message = "帳號或密碼錯誤"
            });
        }


        return Ok(result);
    }
}
