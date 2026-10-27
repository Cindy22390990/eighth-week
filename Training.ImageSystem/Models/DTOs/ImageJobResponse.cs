namespace Training.ImageSystem.Models.DTOs;


public class ImageJobResponse
{
    public int Id { get; set; }


    public string Status { get; set; } = "";


    public string InputPath { get; set; } = "";


    public string? OutputPath { get; set; }


    public string? ErrorCode { get; set; }


    public string? ErrorMessage { get; set; }


    public int? X { get; set; }


    public int? Y { get; set; }


    public int? Width { get; set; }


    public int? Height { get; set; }


    public DateTime CreatedAtUtc { get; set; }


    public DateTime? CompletedAtUtc { get; set; }
}