// QingTianWallPaper.UI/AppBootstrapper.cs
using Autofac;
using MahApps.Metro.Controls.Dialogs;
using QingTianWallPaper.Core.Models;
using QingTianWallPaper.Core.Services;
using QingTianWallPaper.Core.Services.Implementations;
using QingTianWallPaper.Core.Services.Interfaces;
using QingTianWallPaper.QingTianWallPaper.Core.Services.Implementations;
using QingTianWallPaper.QingTianWallPaper.Core.Services.Interfaces;
using QingTianWallPaper.QingTianWallPaper.UI.Views;
using QingTianWallPaper.UI.ViewModels;
using QingTianWallPaper.UI.Views;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace QingTianWallPaper.UI
{
    public class AppBootstrapper : ReactiveObject, IScreen
    {
        private readonly IContainer _container;

        // 路由管理器
        public RoutingState Router { get; }

        public AppBootstrapper()
        {
            Router = new RoutingState();

            // 配置依赖注入
            var builder = new ContainerBuilder();
            ConfigureDependencies(builder);
            _container = builder.Build();

            // 注册视图映射
            RegisterViewModels();

            // 启动应用，导航到主页面
            ShowMainWindow();
        }

        #region 依赖注入配置

        private void ConfigureDependencies(ContainerBuilder builder)
        {
            // 注册服务
            builder.RegisterType<WallpaperService>().As<IWallpaperService>().SingleInstance();
            builder.RegisterType<UserService>().As<IUserService>().SingleInstance();
            builder.RegisterType<DialogCoordinator>().As<IDialogCoordinator>().SingleInstance();

            // 注册视图模型
            builder.RegisterType<AppBootstrapper>().As<IScreen>().SingleInstance();
            builder.RegisterType<MainWindowViewModel>().SingleInstance();
            builder.RegisterType<HomeViewModel>().InstancePerDependency();
            builder.RegisterType<BrowseViewModel>().InstancePerDependency();
            builder.RegisterType<UploadViewModel>().InstancePerDependency();
            builder.RegisterType<ReviewViewModel>().InstancePerDependency();

            // 注册视图
            builder.RegisterType<MainWindow>().SingleInstance();
            builder.RegisterType<HomeView>().AsSelf().InstancePerDependency();
            builder.RegisterType<BrowseView>().AsSelf().InstancePerDependency();
            builder.RegisterType<UploadView>().AsSelf().InstancePerDependency();
            builder.RegisterType<ReviewView>().AsSelf().InstancePerDependency();
        }

        #endregion

        #region 视图模型注册

        private void RegisterViewModels()
        {
            // 注册视图模型与视图的映射关系
            ViewLocator.Current = new ViewLocator();
        }

        #endregion

        #region 导航方法

        public void ShowMainWindow()
        {
            var mainWindow = _container.Resolve<MainWindow>();
            mainWindow.DataContext = _container.Resolve<MainWindowViewModel>();
            mainWindow.Show();
        }

        public void NavigateToHome()
        {
            Router.Navigate.Execute(_container.Resolve<HomeViewModel>()).Subscribe();
        }

        public void NavigateToBrowse()
        {
            var wallpapers = _container.Resolve<IWallpaperService>().GetApprovedWallpapersAsync().Result;
            var browseViewModel = _container.Resolve<BrowseViewModel>();
            Router.Navigate.Execute(browseViewModel).Subscribe();
        }

        public void NavigateToUpload()
        {
            var currentUser = _container.Resolve<IUserService>().GetCurrentUserAsync().Result;
            if (currentUser == null)
            {
                MessageBox.Show("请先登录", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var uploadViewModel = _container.Resolve<UploadViewModel>();
            Router.Navigate.Execute(uploadViewModel).Subscribe();
        }

        public void NavigateToReview()
        {
            var currentUser = _container.Resolve<IUserService>().GetCurrentUserAsync().Result;
            if (currentUser == null)
            {
                MessageBox.Show("请先登录", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!currentUser.IsAdmin && !currentUser.IsReviewer)
            {
                MessageBox.Show("您没有审核权限", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var pendingWallpapers = _container.Resolve<IWallpaperService>().GetPendingWallpapersAsync().Result;
            var reviewViewModel = _container.Resolve<ReviewViewModel>(
                new TypedParameter(typeof(IEnumerable<Wallpaper>), pendingWallpapers),
                new TypedParameter(typeof(User), currentUser));

            Router.Navigate.Execute(reviewViewModel).Subscribe();
        }

        #endregion

        #region 应用程序启动

        public static void StartApplication()
        {
            var appBootstrapper = new AppBootstrapper();
        }

        #endregion
    }

    // 视图定位器，用于将视图模型映射到对应的视图
    public class ViewLocator : IViewLocator
    {
        public IViewFor ResolveView<T>(T viewModel, string contract = null)
        {
            var viewName = viewModel.GetType().FullName.Replace("ViewModel", "View");
            var viewType = Type.GetType(viewName);

            if (viewType != null)
            {
                return (IViewFor)Activator.CreateInstance(viewType);
            }

            return new DefaultViewFor<T>();
        }
    }

    // 默认视图实现
    public class DefaultViewFor<T> : ReactiveUserControl<T>, IViewFor<T>
        where T : class
    {
        public DefaultViewFor()
        {
            this.Content = new TextBlock
            {
                Text = $"未找到视图: {typeof(T).Name}",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            };
        }
    }
}