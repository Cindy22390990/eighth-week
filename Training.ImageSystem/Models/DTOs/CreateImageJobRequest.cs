using Microsoft.AspNetCore.Http;

namespace Training.ImageSystem.Models.DTOs;

public class CreateImageJobRequest
{
    public IFormFile File { get; set; } = null!;
}
