// Repositories/Implementations/WallpaperRepository.cs
using Microsoft.EntityFrameworkCore;
using QingTianWallPaper.Core.Models;
using QingTianWallPaper.Core.Repositories.Interfaces;
using QingTianWallPaper.Data;

namespace QingTianWallPaper.Core.Repositories.Implementations
{
    /// <summary>
    /// 壁纸仓储实现，负责壁纸实体的数据访问
    /// </summary>
    public class WallpaperRepository : IWallpaperRepository
    {
        private readonly AppDbContext _dbContext;

        public WallpaperRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region 基础CRUD方法
        public async Task<IEnumerable<Wallpaper>> GetAllWallpapersAsync(bool includeNavigationProperties = false)
        {
            var query = _dbContext.Wallpapers.AsQueryable();
            if (includeNavigationProperties)
            {
                query = query.Include(w => w.Uploader);
            }
            return await query.ToListAsync();
        }

        public async Task<Wallpaper?> GetWallpaperByIdAsync(int id, bool includeNavigationProperties = false)
        {
            var query = _dbContext.Wallpapers.AsQueryable();
            if (includeNavigationProperties)
            {
                query = query.Include(w => w.Uploader);
            }
            return await query.FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<Wallpaper> AddWallpaperAsync(Wallpaper wallpaper)
        {
            _dbContext.Wallpapers.Add(wallpaper);
            await _dbContext.SaveChangesAsync();
            return wallpaper;
        }

        public async Task<Wallpaper> UpdateWallpaperAsync(Wallpaper wallpaper)
        {
            _dbContext.Entry(wallpaper).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return wallpaper;
        }

        public async Task<bool> DeleteWallpaperAsync(int id)
        {
            var wallpaper = await _dbContext.Wallpapers.FindAsync(id);
            if (wallpaper == null)
            {
                return false;
            }

            // 软删除（标记为已删除）
            wallpaper.IsDeleted = true;
            return await _dbContext.SaveChangesAsync() > 0;
        }
        #endregion

        #region 扩展查询方法
        public async Task<PagedResult<Wallpaper>> GetPendingWallpapersAsync(int page = 1, int pageSize = 10)
        {
            var query = _dbContext.Wallpapers
                .Where(w => w.ReviewStatus == ReviewStatus.Pending && !w.IsDeleted)
                .Include(w => w.Uploader)
                .OrderByDescending(w => w.UploadTime);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Wallpaper>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };
        }

        public async Task<PagedResult<Wallpaper>> GetApprovedWallpapersAsync(int page = 1, int pageSize = 10)
        {
            var query = _dbContext.Wallpapers
                .Where(w => w.ReviewStatus == ReviewStatus.Approved && !w.IsDeleted)
                .Include(w => w.Uploader)
                .OrderByDescending(w => w.UploadTime);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Wallpaper>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };
        }

        public async Task<PagedResult<Wallpaper>> GetWallpapersByTypeAsync(WallpaperType type, int page = 1, int pageSize = 10)
        {
            var query = _dbContext.Wallpapers
                .Where(w => w.Type == type && w.ReviewStatus == ReviewStatus.Approved && !w.IsDeleted)
                .Include(w => w.Uploader)
                .OrderByDescending(w => w.UploadTime);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Wallpaper>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };
        }

        public async Task<PagedResult<Wallpaper>> SearchWallpapersAsync(string keyword, int page = 1, int pageSize = 10)
        {
            var query = _dbContext.Wallpapers
                .Where(w =>
                    (w.Title.Contains(keyword) || w.Description.Contains(keyword)) &&
                    w.ReviewStatus == ReviewStatus.Approved &&
                    !w.IsDeleted)
                .Include(w => w.Uploader)
                .OrderByDescending(w => w.UploadTime);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Wallpaper>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };
        }

        public async Task<PagedResult<Wallpaper>> GetWallpapersByUserAsync(int userId, int page = 1, int pageSize = 10)
        {
            var query = _dbContext.Wallpapers
                .Where(w => w.UploaderId == userId && !w.IsDeleted)
                .Include(w => w.Uploader)
                .OrderByDescending(w => w.UploadTime);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Wallpaper>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };
        }
        #endregion
    }
}