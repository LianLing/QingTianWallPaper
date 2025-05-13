// QingTianWallPaper.Core/Models/WallpaperReview.cs
using System;

namespace QingTianWallPaper.Core.Models
{
    public class WallpaperReview
    {
        public int Id { get; set; }
        public int WallpaperId { get; set; }
        public int ReviewerId { get; set; }
        public ReviewStatus Status { get; set; }
        public string Comment { get; set; }
        public DateTime ReviewTime { get; set; }

        // 导航属性
        public virtual Wallpaper Wallpaper { get; set; }
        public virtual User Reviewer { get; set; }
    }

    public enum ReviewStatus
    {
        Pending = 0,    // 待审核
        Approved = 1,   // 已通过
        Rejected = 2,   // 已拒绝
        Revised = 3     // 已修改
    }
}