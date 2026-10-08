using Training.ImageSystem.Models;

namespace Training.ImageSystem.Repositories.Interfaces;

public interface IImageJobRepository
{
    Task<ImageJob?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ImageJob job,
        CancellationToken cancellationToken = default);

    Task<ImageJob?> ClaimNextPendingJobAsync(
        CancellationToken cancellationToken = default);

    Task MarkSucceededAsync(
        int jobId,
        int x,
        int y,
        int width,
        int height,
        string outputPath,
        CancellationToken cancellationToken = default);

    Task MarkFailedAsync(
        int jobId,
        string errorCode,
        string errorMessage,
        CancellationToken cancellationToken = default);
}