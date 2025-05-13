// QingTianWallPaper.Core/Services/Interfaces/IUserSession.cs
namespace QingTianWallPaper.Core.Services.Interfaces
{
    public interface IUserSession
    {
        int? UserId { get; set; }
        string Username { get; set; }
        bool IsAdmin { get; set; }
        bool IsAuthenticated { get; }
    }
}