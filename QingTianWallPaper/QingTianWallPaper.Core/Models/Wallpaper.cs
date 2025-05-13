// Wallpaper.cs
using System;

namespace QingTianWallPaper.Core.Models
{
    public class Wallpaper
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public string ThumbnailPath { get; set; }
        public long FileSize { get; set; }
        public string Resolution { get; set; }
        public WallpaperType Type { get; set; }
        public DateTime UploadTime { get; set; }
        public int DownloadCount { get; set; }
        public int LikesCount { get; set; }
        public bool IsPublic { get; set; }
        public ReviewStatus ReviewStatus { get; set; }

        public Guid UploaderId { get; set; }
        public User Uploader { get; set; }
    }

    public enum WallpaperType
    {
        Static,
        Animated,
        Video
    }

    public enum ReviewStatus
    {
        Pending,
        Approved,
        Rejected
    }
}