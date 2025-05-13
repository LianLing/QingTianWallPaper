// IWallpaperService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using QingTianWallPaper.Core.Models;

namespace QingTianWallPaper.Core.Services.Interfaces
{
    public interface IWallpaperService
    {
        Task<IEnumerable<Wallpaper>> GetAllWallpapersAsync();
        Task<IEnumerable<Wallpaper>> GetApprovedWallpapersAsync();
        Task<Wallpaper> GetWallpaperByIdAsync(Guid id);
        Task<Wallpaper> UploadWallpaperAsync(Wallpaper wallpaper);
        Task<IEnumerable<Wallpaper>> GetUserUploadsAsync(Guid userId);
        Task<IEnumerable<Wallpaper>> GetPendingReviewsAsync(int reviewerId);
    }
}