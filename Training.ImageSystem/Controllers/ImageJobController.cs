using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.ImageSystem.Models;
using Training.ImageSystem.Models.DTOs;
using Training.ImageSystem.Repositories.Interfaces;
using Training.ImageSystem.Services.Interfaces;


namespace Training.ImageSystem.Controllers;


[ApiController]
[Authorize]
[Route("api/image-jobs")]
public class ImageJobController : ControllerBase
{

    private readonly IImageJobRepository _repository;
    private readonly IImageJobService _service;


    public ImageJobController(
        IImageJobRepository repository,
        IImageJobService service)
    {
        _repository = repository;
        _service = service;
    }



    // ================================
    // Create Image Job
    // ================================

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateImageJobRequest request,
        CancellationToken cancellationToken)
    {

        if (request.File == null ||
            request.File.Length == 0)
        {
            return BadRequest(new
            {
                message = "File is required"
            });
        }



        var extension =
            Path.GetExtension(
                request.File.FileName)
            .ToLowerInvariant();



        var allowedExtensions =
        new[]
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };



        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new
            {
                message =
                "Only jpg jpeg png webp files allowed"
            });
        }



        if (request.File.Length >
            10 * 1024 * 1024)
        {
            return BadRequest(new
            {
                message =
                "File size must less than 10MB"
            });
        }



        // 實際圖片內容驗證
        if (!await IsValidImageAsync(request.File))
        {
            return BadRequest(new
            {
                message =
                "Invalid image content"
            });
        }



        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }



        var job =
            await _service.CreateJobAsync(
                userId,
                request.File);



        return Ok(new
        {
            jobId = job.Id,

            status =
            job.Status.ToString(),

            message =
            "Image job created"
        });

    }




    // ================================
    // Get Job
    // ================================

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(
        int id,
        CancellationToken cancellationToken)
    {

        var job =
            await _repository.GetByIdAsync(
                id,
                cancellationToken);



        if (job == null)
        {
            return NotFound(new
            {
                message =
                "Job not found"
            });
        }



        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }



        if (job.OwnerUserId != userId &&
            !User.IsInRole("Admin"))
        {
            return Forbid();
        }



        var response =
            new ImageJobResponse
            {

                Id = job.Id,


                Status =
                job.Status.ToString(),



                InputPath =
                GetSafePath(job.InputPath),



                OutputPath =
                GetSafePath(job.OutputPath),



                ErrorCode =
                job.ErrorCode,



                ErrorMessage =
                job.ErrorMessage,



                X =
                job.X,



                Y =
                job.Y,



                Width =
                job.Width,



                Height =
                job.Height,



                CreatedAtUtc =
                job.CreatedAtUtc,



                CompletedAtUtc =
                job.CompletedAtUtc

            };



        return Ok(response);

    }




    // ================================
    // Get Result Image
    // ================================

    [HttpGet("{id}/result")]
    public async Task<IActionResult> GetResult(
        int id,
        CancellationToken cancellationToken)
    {


        var job =
            await _repository.GetByIdAsync(
                id,
                cancellationToken);



        if (job == null)
        {
            return NotFound(new
            {
                message =
                "Job not found"
            });
        }



        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }



        if (job.OwnerUserId != userId &&
           !User.IsInRole("Admin"))
        {
            return Forbid();
        }



        if (job.Status != JobStatus.Succeeded)
        {
            return BadRequest(new
            {
                message =
                "Image processing not completed",

                status =
                job.Status.ToString()
            });
        }



        if (string.IsNullOrEmpty(job.OutputPath))
        {
            return BadRequest(new
            {
                message =
                "Image result not ready"
            });
        }



        if (!System.IO.File.Exists(job.OutputPath))
        {
            return NotFound(new
            {
                message =
                "Result image not found"
            });
        }



        var bytes =
            await System.IO.File.ReadAllBytesAsync(
                job.OutputPath,
                cancellationToken);



        var contentType =
            Path.GetExtension(job.OutputPath)
            .ToLowerInvariant()
            switch
            {
                ".png" =>
                "image/png",

                ".jpg" =>
                "image/jpeg",

                ".jpeg" =>
                "image/jpeg",

                _ =>
                "application/octet-stream"
            };



        return File(
            bytes,
            contentType);

    }




    // ================================
    // JWT User ID
    // ================================

    private bool TryGetCurrentUserId(
        out int userId)
    {

        userId = 0;


        var claim =
            User.FindFirst(
                ClaimTypes.NameIdentifier);



        return claim != null &&
               int.TryParse(
                    claim.Value,
                    out userId);

    }




    // ================================
    // Hide Server Absolute Path
    // ================================

    private string? GetSafePath(
        string? path)
    {

        if (string.IsNullOrEmpty(path))
        {
            return null;
        }


        var storage =
            Directory.GetCurrentDirectory();



        if (path.StartsWith(storage))
        {

            return path
                .Replace(
                    storage,
                    "")
                .TrimStart(
                    '\\',
                    '/');

        }


        return path;

    }





    // ================================
    // Image Signature Validation
    // ================================

    private async Task<bool> IsValidImageAsync(
    IFormFile file)
    {
        await using var stream =
            file.OpenReadStream();


        byte[] header =
            new byte[12];


        try
        {
            await stream.ReadExactlyAsync(header);
        }
        catch
        {
            return false;
        }



        // PNG
        if (
            header[0] == 0x89 &&
            header[1] == 0x50 &&
            header[2] == 0x4E &&
            header[3] == 0x47 &&
            header[4] == 0x0D &&
            header[5] == 0x0A &&
            header[6] == 0x1A &&
            header[7] == 0x0A
        )
        {
            return true;
        }



        // JPEG
        if (
            header[0] == 0xFF &&
            header[1] == 0xD8 &&
            header[2] == 0xFF
        )
        {
            return true;
        }



        // WEBP
        if (
            header[0] == (byte)'R' &&
            header[1] == (byte)'I' &&
            header[2] == (byte)'F' &&
            header[3] == (byte)'F' &&

            header[8] == (byte)'W' &&
            header[9] == (byte)'E' &&
            header[10] == (byte)'B' &&
            header[11] == (byte)'P'
        )
        {
            return true;
        }



        return false;
    }





}

