using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Training.ImageSystem.Models.DTOs;
using Training.ImageSystem.Services.Interfaces;


namespace Training.ImageSystem.Controllers;


[ApiController]
[Route("api/image-jobs")]
[Authorize]
public class ImageJobsController : ControllerBase
{
    private readonly IImageJobService _imageJobService;


    public ImageJobsController(
        IImageJobService imageJobService)
    {
        _imageJobService = imageJobService;
    }



    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateImageJobRequest request,
        CancellationToken cancellationToken)
    {

        var userIdClaim =
            User.FindFirst(
                ClaimTypes.NameIdentifier);


        if (userIdClaim == null)
        {
            return Unauthorized();
        }


        var userId =
            int.Parse(userIdClaim.Value);



        var job =
            await _imageJobService.CreateJobAsync(
                userId,
                request.File);



        return Ok(new
        {
            job.Id,
            job.Status,
            job.CreatedAtUtc
        });
    }
}