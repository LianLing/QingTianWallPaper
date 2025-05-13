// QingTianWallPaper.Core/Services/UserSession.cs
using QingTianWallPaper.Core.Services.Interfaces;

namespace QingTianWallPaper.Core.Services
{
    public class UserSession : IUserSession
    {
        public int? UserId { get; set; }
        public string Username { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsAuthenticated => UserId.HasValue;
    }
}