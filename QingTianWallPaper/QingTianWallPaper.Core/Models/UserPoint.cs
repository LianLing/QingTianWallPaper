// QingTianWallPaper.Core/Models/UserPoint.cs
using System;

namespace QingTianWallPaper.Core.Models
{
    public class UserPoint
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Points { get; set; }
        public PointAction Action { get; set; }
        public string Description { get; set; }
        public DateTime CreateTime { get; set; }

        // 导航属性
        public virtual User User { get; set; }
    }

    public enum PointAction
    {
        UploadWallpaper = 1,    // 上传壁纸
        WallpaperApproved = 2,  // 壁纸审核通过
        DownloadWallpaper = 3,  // 下载壁纸
        DailyLogin = 4,         // 每日登录
        InviteFriend = 5,       // 邀请好友
        PremiumSubscription = 6 // 高级订阅
    }
}