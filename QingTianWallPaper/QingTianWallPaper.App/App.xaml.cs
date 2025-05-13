using System;
using System.Windows;
using QingTianWallPaper.UI.Views;
using QingTianWallPaper.UI.ViewModels;
using QingTianWallPaper.Core.Services;
using QingTianWallPaper.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using QingTianWallPaper.Core.Services.Implementations;
using QingTianWallPaper.Core.Services.Interfaces;
using QingTianWallPaper.Data;

namespace QingTianWallPaper
{
    public partial class App : Application
    {
        // 服务提供器
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            // 配置依赖注入
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        // 配置应用服务
        private void ConfigureServices(IServiceCollection services)
        {
            // 数据库上下文
            services.AddDbContext<AppDbContext>();

            // 服务注册
            services.AddSingleton<IWallpaperService, WallpaperService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IRatingService, RatingService>();
            services.AddSingleton<IDownloadService, DownloadService>();

            // 视图模型注册
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<WallpaperListViewModel>();
            services.AddSingleton<WallpaperDetailViewModel>();
            services.AddSingleton<UserViewModel>();
            services.AddSingleton<SettingsViewModel>();

            // 视图注册
            services.AddSingleton<MainWindow>();
            services.AddSingleton<WallpaperListView>();
            services.AddSingleton<WallpaperDetailView>();
            services.AddSingleton<UserView>();
            services.AddSingleton<SettingsView>();
        }

        // 应用启动时
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 确保数据库存在并更新到最新迁移
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureCreated();
                // 这里可以添加数据初始化逻辑
            }

            // 显示主窗口
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        // 应用退出时
        protected override void OnExit(ExitEventArgs e)
        {
            // 释放资源
            _serviceProvider.Dispose();
            base.OnExit(e);
        }
    }
}