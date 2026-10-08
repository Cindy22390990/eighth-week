using Microsoft.AspNetCore.Http;
using Training.ImageSystem.Models;


namespace Training.ImageSystem.Services.Interfaces;


public interface IImageJobService
{
    Task<ImageJob> CreateJobAsync(
        int userId,
        IFormFile file);
}