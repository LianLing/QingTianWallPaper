// QingTianWallPaper.Core/Services/UserService.cs
using Microsoft.EntityFrameworkCore;
using QingTianWallPaper.Core.Models;
using QingTianWallPaper.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QingTianWallPaper.Core.Services
{
    public class UserService : IUserService
    {
        private readonly QingTianDbContext _dbContext;
        private readonly IUserSession _userSession;

        public UserService(QingTianDbContext dbContext, IUserSession userSession)
        {
            _dbContext = dbContext;
            _userSession = userSession;
        }

        public async Task<UserEntity> GetUserByIdAsync(int userId)
        {
            return await _dbContext.Users.FindAsync(userId);
        }

        public async Task<UserEntity> GetUserByUsernameAsync(string username)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<UserEntity> GetUserByEmailAsync(string email)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<UserEntity> GetCurrentUserAsync()
        {
            if (_userSession.UserId.HasValue)
            {
                return await GetUserByIdAsync(_userSession.UserId.Value);
            }
            return null;
        }

        public async Task<bool> RegisterUserAsync(string username, string email, string password)
        {
            // 检查用户名或邮箱是否已存在
            if (await _dbContext.Users.AnyAsync(u => u.Username == username || u.Email == email))
            {
                return false;
            }

            // 创建新用户
            var user = new UserEntity
            {
                Username = username,
                Email = email,
                PasswordHash = HashPassword(password),
                AvatarUrl = $"https://picsum.photos/seed/{username}/200/200", // 默认头像
                Points = 0,
                IsAdmin = false,
                IsReviewer = false,
                IsPremium = false,
                RegisterTime = DateTime.Now,
                LastLoginTime = DateTime.Now,
                IsActive = true
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // 记录注册积分
            var pointService = new UserPointService(_dbContext);
            await pointService.AddPointsAsync(
                user.Id,
                20,
                PointAction.DailyLogin,
                "注册成功获得20积分");

            return true;
        }

        public async Task<UserEntity> AuthenticateAsync(string username, string password)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                return null;
            }

            // 更新最后登录时间
            user.LastLoginTime = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            // 记录用户会话
            _userSession.UserId = user.Id;
            _userSession.Username = user.Username;
            _userSession.IsAdmin = user.IsAdmin;

            // 检查是否为每日首次登录并给予积分
            var pointService = new UserPointService(_dbContext);
            var lastLoginDate = user.LastLoginTime.Date;
            var today = DateTime.Now.Date;

            if (lastLoginDate < today)
            {
                await pointService.AddPointsAsync(
                    user.Id,
                    5,
                    PointAction.DailyLogin,
                    "每日登录获得5积分");
            }

            return user;
        }

        public async Task UpdateUserAsync(UserEntity user)
        {
            var existingUser = await _dbContext.Users.FindAsync(user.Id);

            if (existingUser == null)
            {
                throw new ArgumentException("用户不存在", nameof(user.Id));
            }

            // 更新可修改的属性
            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.AvatarUrl = user.AvatarUrl;
            existingUser.Bio = user.Bio;
            existingUser.IsAdmin = user.IsAdmin;
            existingUser.IsReviewer = user.IsReviewer;
            existingUser.IsPremium = user.IsPremium;
            existingUser.IsActive = user.IsActive;

            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _dbContext.Users.FindAsync(userId);

            if (user == null || !VerifyPassword(oldPassword, user.PasswordHash))
            {
                return false;
            }

            user.PasswordHash = HashPassword(newPassword);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserEntity>> GetAllUsersAsync(int page = 1, int pageSize = 20)
        {
            return await _dbContext.Users
                .OrderByDescending(u => u.RegisterTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);

            if (user == null)
            {
                return false;
            }

            // 逻辑删除用户
            user.IsActive = false;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        #region 密码哈希与验证

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            return HashPassword(password) == passwordHash;
        }

        #endregion
    }
}