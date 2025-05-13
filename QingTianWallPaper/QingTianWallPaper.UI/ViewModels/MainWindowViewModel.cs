// MainWindowViewModel.cs
using MahApps.Metro.Controls.Dialogs;
using QingTianWallPaper.Core.Models;
using QingTianWallPaper.Core.Services.Interfaces;
using QingTianWallPaper.QingTianWallPaper.UI.ViewModels;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;

namespace QingTianWallPaper.UI.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IWallpaperService _wallpaperService;
        private readonly IUserService _userService;

        public MainWindowViewModel(
            IDialogCoordinator dialogCoordinator,
            IWallpaperService wallpaperService,
            IUserService userService)
        {
            _dialogCoordinator = dialogCoordinator;
            _wallpaperService = wallpaperService;
            _userService = userService;

            // 初始化命令
            NavigateHomeCommand = ReactiveCommand.Create(NavigateHome);
            NavigateBrowseCommand = ReactiveCommand.Create(NavigateBrowse);
            NavigateUploadCommand = ReactiveCommand.CreateFromTask(NavigateUploadAsync);
            NavigateReviewsCommand = ReactiveCommand.CreateFromTask(NavigateReviewsAsync);

            // 默认显示主页
            CurrentView = new HomeViewModel(_wallpaperService);
            StatusMessage = "欢迎使用 QingTianWallPaper";

            // 加载用户信息
            LoadUserInfo();
        }

        // 当前显示的视图
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }

        // 状态栏消息
        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        // 用户信息
        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set => this.RaiseAndSetIfChanged(ref _currentUser, value);
        }

        // 导航命令
        public ReactiveCommand<Unit, Unit> NavigateHomeCommand { get; }
        public ReactiveCommand<Unit, Unit> NavigateBrowseCommand { get; }
        public ReactiveCommand<Unit, Task> NavigateUploadCommand { get; }
        public ReactiveCommand<Unit, Task> NavigateReviewsCommand { get; }

        // 导航方法
        private void NavigateHome()
        {
            CurrentView = new HomeViewModel(_wallpaperService);
            StatusMessage = "已切换到主页";
        }

        private void NavigateBrowse()
        {
            CurrentView = new BrowseViewModel(_wallpaperService);
            StatusMessage = "已切换到浏览页面";
        }

        private async Task NavigateUploadAsync()
        {
            if (CurrentUser == null)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "未登录", "请先登录以使用上传功能");
                return;
            }

            CurrentView = new UploadViewModel(_wallpaperService, _userService, CurrentUser);
            StatusMessage = "已切换到上传页面";
        }

        private async Task NavigateReviewsAsync()
        {
            if (CurrentUser == null)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "未登录", "请先登录以查看审核任务");
                return;
            }

            var pendingReviews = await _wallpaperService.GetPendingReviewsAsync(CurrentUser.Id);
            if (!pendingReviews.Any())
            {
                await _dialogCoordinator.ShowMessageAsync(this, "没有任务", "当前没有待审核的壁纸");
                return;
            }

            CurrentView = new ReviewViewModel(_wallpaperService, _userService, CurrentUser, pendingReviews);
            StatusMessage = "已切换到审核页面";
        }

        private async void LoadUserInfo()
        {
            // 实际应用中应该从认证服务获取当前用户
            // 这里为简化示例，假设用户已登录
            CurrentUser = await _userService.GetCurrentUserAsync();
        }
    }
}