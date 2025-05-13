// Repositories/Implementations/UserRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QingTianWallPaper.Core.Entities;
using QingTianWallPaper.Core.Models;
using QingTianWallPaper.Core.Repositories.Interfaces;
using QingTianWallPaper.Data;
using QingTianWallPaper.Infrastructure.Data;

namespace QingTianWallPaper.Core.Repositories.Implementations
{
    /// <summary>
    /// 用户仓储实现，继承自通用仓储基类（假设存在）或直接操作DbContext
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext; // 假设存在应用程序DbContext

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _dbContext.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _dbContext.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> AddUserAsync(User user)
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            _dbContext.Entry(user).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null) return false;

            _dbContext.Users.Remove(user);
            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}