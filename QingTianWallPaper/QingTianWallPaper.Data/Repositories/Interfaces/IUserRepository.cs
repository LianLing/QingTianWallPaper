// Repositories/Interfaces/IUserRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QingTianWallPaper.Core.Entities;
using QingTianWallPaper.Core.Models;

namespace QingTianWallPaper.Core.Repositories.Interfaces
{
    /// <summary>
    /// 用户仓储接口，定义用户相关数据访问契约
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// 异步获取所有用户
        /// </summary>
        /// <returns>用户集合</returns>
        Task<IEnumerable<User>> GetAllUsersAsync();

        /// <summary>
        /// 异步根据ID获取用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>用户实体，若不存在则返回null</returns>
        Task<User?> GetUserByIdAsync(Guid id);

        /// <summary>
        /// 异步根据用户名获取用户
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns>用户实体，若不存在则返回null</returns>
        Task<User?> GetUserByUsernameAsync(string username);

        /// <summary>
        /// 异步添加用户
        /// </summary>
        /// <param name="user">用户实体</param>
        /// <returns>添加后的用户实体</returns>
        Task<User> AddUserAsync(User user);

        /// <summary>
        /// 异步更新用户
        /// </summary>
        /// <param name="user">用户实体</param>
        /// <returns>更新后的用户实体</returns>
        Task<User> UpdateUserAsync(User user);

        /// <summary>
        /// 异步删除用户（可根据需求实现软删除或硬删除）
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>是否删除成功</returns>
        Task<bool> DeleteUserAsync(Guid id);
    }
}