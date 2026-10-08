namespace Training.ImageSystem.Models;

public class ImageJob
{
    public int Id { get; set; }
    public int OwnerUserId {  get; set; }
    public string InputPath {  get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public JobStatus Status { get; set; }
    public string? ErrorCode {  get; set; }
    public string? ErrorMessage {  get; set; }
    public int? X {  get; set; }
    public int? Y { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public DateTime CreatedAtUtc {  get; set; }
    public DateTime? StartedAtUtc {  get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = null!;
}
