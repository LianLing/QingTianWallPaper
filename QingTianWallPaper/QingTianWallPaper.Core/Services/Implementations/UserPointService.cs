// QingTianWallPaper.Core/Services/UserPointService.cs
using Microsoft.EntityFrameworkCore;
using QingTianWallPaper.Core.Models;
using QingTianWallPaper.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QingTianWallPaper.Core.Services
{
    public class UserPointService : IUserPointService
    {
        private readonly QingTianDbContext _dbContext;

        public UserPointService(QingTianDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddPointsAsync(int userId, int points, PointAction action, string description = null)
        {
            // 检查用户是否存在
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("用户不存在", nameof(userId));
            }

            // 添加积分记录
            var pointRecord = new UserPoint
            {
                UserId = userId,
                Points = points,
                Action = action,
                Description = description ?? GetDefaultDescription(action),
                CreateTime = DateTime.Now
            };

            _dbContext.UserPoints.Add(pointRecord);

            // 更新用户总积分
            user.Points += points;

            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetUserPointsAsync(int userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            return user?.Points ?? 0;
        }

        public async Task<List<UserPoint>> GetUserPointHistoryAsync(int userId, int page = 1, int pageSize = 20)
        {
            return await _dbContext.UserPoints
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> DeductPointsAsync(int userId, int points, string description = null)
        {
            // 检查用户是否存在且积分足够
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null || user.Points < points)
            {
                return false;
            }

            // 添加积分扣除记录
            var pointRecord = new UserPoint
            {
                UserId = userId,
                Points = -points, // 负分表示扣除
                Action = PointAction.DownloadWallpaper, // 默认使用下载壁纸动作
                Description = description ?? "兑换积分",
                CreateTime = DateTime.Now
            };

            _dbContext.UserPoints.Add(pointRecord);

            // 更新用户总积分
            user.Points -= points;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        private string GetDefaultDescription(PointAction action)
        {
            return action switch
            {
                PointAction.UploadWallpaper => "上传壁纸获得积分",
                PointAction.WallpaperApproved => "壁纸审核通过获得积分",
                PointAction.DownloadWallpaper => "下载壁纸消耗积分",
                PointAction.DailyLogin => "每日登录获得积分",
                PointAction.InviteFriend => "邀请好友获得积分",
                PointAction.PremiumSubscription => "高级订阅获得积分",
                _ => "积分变动"
            };
        }
    }
}