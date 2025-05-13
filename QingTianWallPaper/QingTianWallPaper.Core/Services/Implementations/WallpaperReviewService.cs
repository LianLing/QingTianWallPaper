// QingTianWallPaper.Core/Services/WallpaperReviewService.cs
using Microsoft.EntityFrameworkCore;
using QingTianWallPaper.Core.Models;
using QingTianWallPaper.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QingTianWallPaper.Core.Services
{
    public class WallpaperReviewService : IWallpaperReviewService
    {
        private readonly QingTianDbContext _dbContext;
        private readonly IUserPointService _userPointService;

        public WallpaperReviewService(QingTianDbContext dbContext, IUserPointService userPointService)
        {
            _dbContext = dbContext;
            _userPointService = userPointService;
        }

        public async Task<List<Wallpaper>> GetPendingWallpapersAsync(int page = 1, int pageSize = 20)
        {
            return await _dbContext.Wallpapers
                .Include(w => w.Uploader)
                .Where(w => w.ReviewStatus == ReviewStatus.Pending)
                .OrderByDescending(w => w.UploadTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<WallpaperReview> GetReviewByWallpaperIdAsync(int wallpaperId)
        {
            return await _dbContext.WallpaperReviews
                .FirstOrDefaultAsync(r => r.WallpaperId == wallpaperId);
        }

        public async Task ApproveWallpaperAsync(int wallpaperId, int reviewerId, string comment = null)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 更新壁纸状态
                var wallpaper = await _dbContext.Wallpapers.FindAsync(wallpaperId);
                if (wallpaper == null)
                {
                    throw new ArgumentException("壁纸不存在", nameof(wallpaperId));
                }

                wallpaper.ReviewStatus = ReviewStatus.Approved;
                wallpaper.ReviewTime = DateTime.Now;

                // 添加审核记录
                var review = new WallpaperReview
                {
                    WallpaperId = wallpaperId,
                    ReviewerId = reviewerId,
                    Status = ReviewStatus.Approved,
                    Comment = comment,
                    ReviewTime = DateTime.Now
                };

                _dbContext.WallpaperReviews.Add(review);

                // 为上传者添加积分
                await _userPointService.AddPointsAsync(
                    wallpaper.UploaderId,
                    10,
                    PointAction.WallpaperApproved,
                    "壁纸审核通过获得10积分");

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RejectWallpaperAsync(int wallpaperId, int reviewerId, string comment = null)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 更新壁纸状态
                var wallpaper = await _dbContext.Wallpapers.FindAsync(wallpaperId);
                if (wallpaper == null)
                {
                    throw new ArgumentException("壁纸不存在", nameof(wallpaperId));
                }

                wallpaper.ReviewStatus = ReviewStatus.Rejected;
                wallpaper.ReviewTime = DateTime.Now;

                // 添加审核记录
                var review = new WallpaperReview
                {
                    WallpaperId = wallpaperId,
                    ReviewerId = reviewerId,
                    Status = ReviewStatus.Rejected,
                    Comment = comment,
                    ReviewTime = DateTime.Now
                };

                _dbContext.WallpaperReviews.Add(review);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Wallpaper>> GetReviewedWallpapersAsync(int reviewerId, int page = 1, int pageSize = 20)
        {
            return await _dbContext.Wallpapers
                .Include(w => w.Uploader)
                .Include(w => w.Reviews)
                .Where(w => w.Reviews.Any(r => r.ReviewerId == reviewerId))
                .OrderByDescending(w => w.ReviewTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}