// User.cs
using System;
using System.Collections.Generic;
using QingTianWallPaper.QingTianWallPaper.Core.Models;

namespace QingTianWallPaper.Core.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public int Points { get; set; }
        
        public List<Wallpaper> UploadedWallpapers { get; set; } = new();
        public List<WallpaperReview> Reviews { get; set; } = new();
    }
}