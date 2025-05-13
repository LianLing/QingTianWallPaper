// QingTianWallPaper.Core/Models/UserEntity.cs
using System;
using System.Collections.Generic;

namespace QingTianWallPaper.Core.Models
{
    public class UserEntity
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public string Bio { get; set; }
        public int Points { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsReviewer { get; set; }
        public bool IsPremium { get; set; }
        public DateTime RegisterTime { get; set; }
        public DateTime LastLoginTime { get; set; }
        public bool IsActive { get; set; }

        // 导航属性
        public virtual ICollection<Wallpaper> UploadedWallpapers { get; set; }
        public virtual ICollection<WallpaperReview> Reviews { get; set; }
        public virtual ICollection<UserPoint> PointHistories { get; set; }
    }
}