// Repositories/Interfaces/IWallpaperRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using QingTianWallPaper.Core.Entities;
using QingTianWallPaper.Core.Models;
using QingTianWallPaper.Shared.Pagination;

namespace QingTianWallPaper.Core.Repositories.Interfaces
{
    /// <summary>
    /// 壁纸仓储接口，定义壁纸相关数据访问契约
    /// </summary>
    public interface IWallpaperRepository
    {
        #region 基础CRUD方法
        /// <summary>
        /// 异步获取所有壁纸（含导航属性）
        /// </summary>
        /// <param name="includeNavigationProperties">是否包含导航属性（如上传者）</param>
        /// <returns>壁纸集合</returns>
        Task<IEnumerable<Wallpaper>> GetAllWallpapersAsync(bool includeNavigationProperties = false);

        /// <summary>
        /// 异步根据ID获取壁纸（含导航属性）
        /// </summary>
        /// <param name="id">壁纸ID</param>
        /// <param name="includeNavigationProperties">是否包含导航属性（如上传者）</param>
        /// <returns>壁纸实体，若不存在则返回null</returns>
        Task<Wallpaper?> GetWallpaperByIdAsync(int id, bool includeNavigationProperties = false);

        /// <summary>
        /// 异步添加壁纸
        /// </summary>
        /// <param name="wallpaper">壁纸实体</param>
        /// <returns>添加后的壁纸实体</returns>
        Task<Wallpaper> AddWallpaperAsync(Wallpaper wallpaper);

        /// <summary>
        /// 异步更新壁纸
        /// </summary>
        /// <param name="wallpaper">壁纸实体</param>
        /// <returns>更新后的壁纸实体</returns>
        Task<Wallpaper> UpdateWallpaperAsync(Wallpaper wallpaper);

        /// <summary>
        /// 异步删除壁纸（可根据需求实现软删除或硬删除）
        /// </summary>
        /// <param name="id">壁纸ID</param>
        /// <returns>是否删除成功</returns>
        Task<bool> DeleteWallpaperAsync(int id);
        #endregion

        #region 扩展查询方法
        /// <summary>
        /// 异步获取待审核的壁纸（支持分页）
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <returns>分页结果</returns>
        Task<PagedResult<Wallpaper>> GetPendingWallpapersAsync(int page = 1, int pageSize = 10);

        /// <summary>
        /// 异步获取已通过审核的壁纸（支持分页）
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <returns>分页结果</returns>
        Task<PagedResult<Wallpaper>> GetApprovedWallpapersAsync(int page = 1, int pageSize = 10);

        /// <summary>
        /// 异步根据类型获取壁纸（支持分页）
        /// </summary>
        /// <param name="type">壁纸类型</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <returns>分页结果</returns>
        Task<PagedResult<Wallpaper>> GetWallpapersByTypeAsync(WallpaperType type, int page = 1, int pageSize = 10);

        /// <summary>
        /// 异步搜索壁纸（按标题或描述）
        /// </summary>
        /// <param name="keyword">搜索关键词</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <returns>分页结果</returns>
        Task<PagedResult<Wallpaper>> SearchWallpapersAsync(string keyword, int page = 1, int pageSize = 10);

        /// <summary>
        /// 异步获取用户上传的壁纸（支持分页）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <returns>分页结果</returns>
        Task<PagedResult<Wallpaper>> GetWallpapersByUserAsync(int userId, int page = 1, int pageSize = 10);
        #endregion
    }
}