// WallpaperService.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QingTianWallPaper.Core.Models;
using QingTianWallPaper.Core.Services.Interfaces;
using QingTianWallPaper.Data.Repositories.Interfaces;

namespace QingTianWallPaper.Core.Services.Implementations
{
    public class WallpaperService : IWallpaperService
    {
        private readonly IWallpaperRepository _wallpaperRepository;
        private readonly IUserRepository _userRepository;
        private readonly IReviewService _reviewService;

        public WallpaperService(
            IWallpaperRepository wallpaperRepository,
            IUserRepository userRepository,
            IReviewService reviewService)
        {
            _wallpaperRepository = wallpaperRepository;
            _userRepository = userRepository;
            _reviewService = reviewService;
        }

        public async Task<IEnumerable<Wallpaper>> GetAllWallpapersAsync()
        {
            return await _wallpaperRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Wallpaper>> GetApprovedWallpapersAsync()
        {
            return await _wallpaperRepository.GetByStatusAsync(ReviewStatus.Approved);
        }

        public async Task<Wallpaper> UploadWallpaperAsync(Wallpaper wallpaper)
        {
            wallpaper.ReviewStatus = ReviewStatus.Pending;
            wallpaper.UploadTime = DateTime.Now;

            var savedWallpaper = await _wallpaperRepository.AddAsync(wallpaper);

            // 自动分配审核任务
            await _reviewService.AssignReviewersAsync(savedWallpaper.Id);

            return savedWallpaper;
        }

        // 其他方法实现...
    }
}