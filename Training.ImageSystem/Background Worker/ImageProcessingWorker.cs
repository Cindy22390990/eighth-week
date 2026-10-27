using Training.ImageSystem.Models;
using Training.ImageSystem.Repositories.Interfaces;
using Training.ImageSystem.Services;


namespace Training.ImageSystem.Background_Worker;


public class ImageProcessingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ILogger<ImageProcessingWorker> _logger;
    private readonly IWebHostEnvironment _environment;


    public ImageProcessingWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ImageProcessingWorker> logger,
        IWebHostEnvironment environment)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _environment = environment;
    }




    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            ImageJob? currentJob = null;

            try
            {

                using var scope =
                    _scopeFactory.CreateScope();



                var repository =
                    scope.ServiceProvider
                    .GetRequiredService<IImageJobRepository>();


                var processor =
                    scope.ServiceProvider
                    .GetRequiredService<PythonImageProcessor>();



                currentJob =
                    await repository
                    .ClaimNextPendingJobAsync(
                        stoppingToken);



                if (currentJob == null)
                {

                    await Task.Delay(
                        5000,
                        stoppingToken);


                    continue;
                }




                _logger.LogInformation(
                    "開始處理 JobId:{JobId}",
                    currentJob.Id);




                var inputPath =
                    Path.Combine(
                        _environment.ContentRootPath,
                        currentJob.InputPath);



                var resultFolder =
                    Path.Combine(
                        _environment.ContentRootPath,
                        "Storage",
                        "Results");



                Directory.CreateDirectory(
                    resultFolder);



                var outputFileName =
                    $"{currentJob.Id}.jpg";



                var outputPhysicalPath =
                    Path.Combine(
                        resultFolder,
                        outputFileName);



                var outputRelativePath =
                    Path.Combine(
                        "Storage",
                        "Results",
                        outputFileName)
                    .Replace("\\", "/");




                var result =
                    await processor.ExecuteAsync(
                        inputPath,
                        outputPhysicalPath,
                        stoppingToken);

                if (result == null)
                {

                    await repository.MarkFailedAsync(
                        currentJob.Id,
                        "PYTHON_EXCEPTION",
                        "Python execution failed",
                        stoppingToken);


                    continue;
                }

                if (!File.Exists(outputPhysicalPath))
                {

                    await repository.MarkFailedAsync(
                        currentJob.Id,
                        "OUTPUT_NOT_FOUND",
                        "Python output file missing",
                        stoppingToken);
                    continue;
                }

                if (result.Status != "success")
                {

                    await repository.MarkFailedAsync(
                        currentJob.Id,
                        "PYTHON_FAILED",
                        result.Message ??
                        "Python returned failed status",
                        stoppingToken);

                    continue;
                }





                if (
                    result.Width <= 0 ||
                    result.Height <= 0 ||
                    result.X < 0 ||
                    result.Y < 0 ||
                    result.ImageWidth <= 0 ||
                    result.ImageHeight <= 0 ||
                    result.X + result.Width > result.ImageWidth ||
                    result.Y + result.Height > result.ImageHeight
                )
                {

                    await repository.MarkFailedAsync(
                        currentJob.Id,
                        "INVALID_POSITION",
                        "Python returned invalid rectangle",
                        stoppingToken);

                    continue;
                }





                await repository.MarkSucceededAsync(
                    currentJob.Id,
                    result.X,
                    result.Y,
                    result.Width,
                    result.Height,
                    outputRelativePath,
                    stoppingToken);



                _logger.LogInformation(
                    "完成處理 JobId:{JobId}",
                    currentJob.Id);

            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {

                _logger.LogError(
                    ex,
                    "Worker failed JobId:{JobId}",
                    currentJob?.Id);


                if (currentJob != null)
                {
                    try
                    {
                        using var scope =
                            _scopeFactory.CreateScope();


                        var repository =
                            scope.ServiceProvider
                            .GetRequiredService<IImageJobRepository>();


                        await repository.MarkFailedAsync(
                            currentJob.Id,
                            "WORKER_EXCEPTION",
                            "Background worker exception",
                            stoppingToken);

                    }
                    catch (Exception innerEx)
                    {

                        _logger.LogError(
                            innerEx,
                            "Failed updating job status JobId:{JobId}",
                            currentJob.Id);

                    }

                }

            }

        }

    }

}