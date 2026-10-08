using Training.ImageSystem.Models;
using Training.ImageSystem.Repositories.Interfaces;
using Training.ImageSystem.Services.Interfaces;
using Microsoft.AspNetCore.Http;


namespace Training.ImageSystem.Services;


public class ImageJobService : IImageJobService
{
    private readonly IImageJobRepository _repository;
    private readonly IWebHostEnvironment _environment;


    public ImageJobService(
        IImageJobRepository repository,
        IWebHostEnvironment environment)
    {
        _repository = repository;
        _environment = environment;
    }



    public async Task<ImageJob> CreateJobAsync(
        int userId,
        IFormFile file)
    {

        var uploadFolder =
            Path.Combine(
                _environment.ContentRootPath,
                "Storage",
                "Uploads");


        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
        }

        var extension =
            Path.GetExtension(file.FileName);

        var fileName =
            $"{Guid.NewGuid()}{extension}";



        var filePath =
            Path.Combine(
                uploadFolder,
                fileName);



        using (var stream = new FileStream(
            filePath,
            FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }



        var job = new ImageJob
        {
            OwnerUserId = userId,

            InputPath = filePath,

            OutputPath = "",

            Status = JobStatus.Pending,

            CreatedAtUtc = DateTime.UtcNow
        };



        await _repository.AddAsync(job);

        


        return job;
    }
}