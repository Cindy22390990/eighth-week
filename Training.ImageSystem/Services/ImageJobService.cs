using Training.ImageSystem.Models;
using Training.ImageSystem.Repositories.Interfaces;
using Training.ImageSystem.Services.Interfaces;


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


        Directory.CreateDirectory(
            uploadFolder);



        var extension =
            Path.GetExtension(
                file.FileName)
            .ToLowerInvariant();



        var fileName =
            $"{Guid.NewGuid()}{extension}";



        // 實際磁碟路徑
        var physicalPath =
            Path.Combine(
                uploadFolder,
                fileName);


        try
        {
            // 寫入圖片
            await using (
                var stream =
                new FileStream(
                    physicalPath,
                    FileMode.Create))
            {
            await file.CopyToAsync(stream);
            }


            // DB 保存相對路徑
            var relativeInputPath =
                Path.Combine(
                    "Storage",
                    "Uploads",
                    fileName)
                .Replace("\\", "/");



            var job =
                new ImageJob
                {

                    OwnerUserId =
                    userId,


                    InputPath =
                    relativeInputPath,


                    OutputPath =
                    string.Empty,


                    Status =
                    JobStatus.Pending,


                    CreatedAtUtc =
                    DateTime.UtcNow
                };



            await _repository.AddAsync(job);



            return job;
        }
        catch
        {

            // SQL 失敗時清理孤兒圖片
            if (File.Exists(physicalPath))
            {
                try
                {
                    File.Delete(physicalPath);
                }
                catch
                {
                    // 避免清理失敗覆蓋原始例外
                }
            }


            throw;
        }

    }
}