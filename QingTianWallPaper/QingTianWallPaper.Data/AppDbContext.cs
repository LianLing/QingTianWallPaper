// AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using QingTianWallPaper.Data.Models;

namespace QingTianWallPaper.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<WallpaperEntity> Wallpapers { get; set; }
        public DbSet<WallpaperReviewEntity> Reviews { get; set; }
        public DbSet<UserPointEntity> Points { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseSqlite("Data Source=QingTianWallPaper.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 应用实体配置
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}