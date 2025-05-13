// QingTianWallPaper.Core/Services/Interfaces/IUserPointService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using QingTianWallPaper.Core.Models;

namespace QingTianWallPaper.Core.Services.Interfaces
{
    public interface IUserPointService
    {
        Task AddPointsAsync(int userId, int points, PointAction action, string description = null);
        Task<int> GetUserPointsAsync(int userId);
        Task<List<UserPoint>> GetUserPointHistoryAsync(int userId, int page = 1, int pageSize = 20);
        Task<bool> DeductPointsAsync(int userId, int points, string description = null);
    }
}