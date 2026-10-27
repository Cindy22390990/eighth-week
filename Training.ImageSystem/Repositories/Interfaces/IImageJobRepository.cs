using Training.ImageSystem.Models;

namespace Training.ImageSystem.Repositories.Interfaces;

public interface IImageJobRepository
{
    /// 根據 Job Id 查詢影像任務
    Task<ImageJob?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// 新增影像任務
    Task AddAsync(
        ImageJob job,
        CancellationToken cancellationToken = default);
    
    /// 取得下一筆 Pending 狀態的任務並鎖定處理
    Task<ImageJob?> ClaimNextPendingJobAsync(
        CancellationToken cancellationToken = default);

    /// 更新任務成功結果
    Task MarkSucceededAsync(
        int jobId,
        int x,
        int y,
        int width,
        int height,
        string outputPath,
        CancellationToken cancellationToken = default);

    /// 更新任務失敗結果
    Task MarkFailedAsync(
        int jobId,
        string errorCode,
        string errorMessage,
        CancellationToken cancellationToken = default);
}