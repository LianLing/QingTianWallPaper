// QingTianWallPaper.Core/Services/Interfaces/IWallpaperReviewService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using QingTianWallPaper.Core.Models;

namespace QingTianWallPaper.Core.Services.Interfaces
{
    public interface IWallpaperReviewService
    {
        Task<List<Wallpaper>> GetPendingWallpapersAsync(int page = 1, int pageSize = 20);
        Task<WallpaperReview> GetReviewByWallpaperIdAsync(int wallpaperId);
        Task ApproveWallpaperAsync(int wallpaperId, int reviewerId, string comment = null);
        Task RejectWallpaperAsync(int wallpaperId, int reviewerId, string comment = null);
        Task<List<Wallpaper>> GetReviewedWallpapersAsync(int reviewerId, int page = 1, int pageSize = 20);
    }
}