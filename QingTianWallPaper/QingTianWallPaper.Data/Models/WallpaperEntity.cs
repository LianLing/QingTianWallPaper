// QingTianWallPaper.Core/Models/WallpaperEntity.cs
using System;
using System.Collections.Generic;

namespace QingTianWallPaper.Core.Models
{
    public class WallpaperEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public string ThumbnailPath { get; set; }
        public string PreviewPath { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string Resolution { get; set; }
        public WallpaperType Type { get; set; }
        public int UploaderId { get; set; }
        public DateTime UploadTime { get; set; }
        public DateTime? ReviewTime { get; set; }
        public ReviewStatus ReviewStatus { get; set; }
        public int DownloadCount { get; set; }
        public int ViewCount { get; set; }
        public bool IsFeatured { get; set; }

        // 导航属性
        public virtual UserEntity Uploader { get; set; }
        public virtual ICollection<WallpaperReview> Reviews { get; set; }
        public virtual ICollection<UserFavorite> UserFavorites { get; set; }
    }

    public enum WallpaperType
    {
        Landscape = 1,  // 风景
        Animal = 2,     // 动物
        People = 3,     // 人物
        Abstract = 4,   // 抽象
        Anime = 5,      // 动漫
        City = 6,       // 城市
        Nature = 7,     // 自然
        Technology = 8  // 科技
    }
}