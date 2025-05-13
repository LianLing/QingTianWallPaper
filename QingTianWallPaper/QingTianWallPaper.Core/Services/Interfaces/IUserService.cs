// QingTianWallPaper.Core/Services/Interfaces/IUserService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using QingTianWallPaper.Core.Models;

namespace QingTianWallPaper.Core.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserEntity> GetUserByIdAsync(int userId);
        Task<UserEntity> GetUserByUsernameAsync(string username);
        Task<UserEntity> GetUserByEmailAsync(string email);
        Task<UserEntity> GetCurrentUserAsync();
        Task<bool> RegisterUserAsync(string username, string email, string password);
        Task<UserEntity> AuthenticateAsync(string username, string password);
        Task UpdateUserAsync(UserEntity user);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<IEnumerable<UserEntity>> GetAllUsersAsync(int page = 1, int pageSize = 20);
        Task<bool> DeleteUserAsync(int userId);
    }
}