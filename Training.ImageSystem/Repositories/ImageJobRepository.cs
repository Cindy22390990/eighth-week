using Microsoft.EntityFrameworkCore;
using Training.ImageSystem.Data;
using Training.ImageSystem.Models;
using Training.ImageSystem.Repositories.Interfaces;

namespace Training.ImageSystem.Repositories;

public class ImageJobRepository : IImageJobRepository
{
    private readonly AppDbContext _context;


    public ImageJobRepository(AppDbContext context)
    {
        _context = context;
    }



    public async Task<ImageJob?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.ImageJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }



    public async Task AddAsync(
        ImageJob job,
        CancellationToken cancellationToken = default)
    {
        await _context.ImageJobs.AddAsync(
            job,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);
    }



    public async Task<ImageJob?> ClaimNextPendingJobAsync(
        CancellationToken cancellationToken = default)
    {
        var job = await _context.ImageJobs
            .FirstOrDefaultAsync(
                x => x.Status == JobStatus.Pending,
                cancellationToken);


        if (job == null)
        {
            return null;
        }


        job.Status = JobStatus.Processing;

        job.StartedAtUtc = DateTime.UtcNow;


        await _context.SaveChangesAsync(
            cancellationToken);


        return job;
    }



    public async Task MarkSucceededAsync(
        int jobId,
        int x,
        int y,
        int width,
        int height,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        var job = await _context.ImageJobs
            .FirstOrDefaultAsync(
                j => j.Id == jobId,
                cancellationToken);


        if (job == null)
        {
            return;
        }


        job.Status = JobStatus.Succeeded;

        job.X = x;
        job.Y = y;
        job.Width = width;
        job.Height = height;

        job.OutputPath = outputPath;

        job.CompletedAtUtc = DateTime.UtcNow;


        job.ErrorCode = null;
        job.ErrorMessage = null;


        await _context.SaveChangesAsync(
            cancellationToken);
    }



    public async Task MarkFailedAsync(
        int jobId,
        string errorCode,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        var job = await _context.ImageJobs
            .FirstOrDefaultAsync(
                j => j.Id == jobId,
                cancellationToken);


        if (job == null)
        {
            return;
        }


        job.Status = JobStatus.Failed;

        job.ErrorCode = errorCode;

        job.ErrorMessage = errorMessage;

        job.CompletedAtUtc = DateTime.UtcNow;


        await _context.SaveChangesAsync(
            cancellationToken);
    }
}