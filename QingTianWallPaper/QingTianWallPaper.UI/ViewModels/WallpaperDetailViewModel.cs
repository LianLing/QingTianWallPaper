// QingTianWallPaper.UI/ViewModels/WallpaperDetailViewModel.cs
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using QingTianWallPaper.Core.Enums;
using QingTianWallPaper.Core.Models;
using QingTianWallPaper.Core.Services.Interfaces;
using ReactiveUI;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace QingTianWallPaper.UI.ViewModels
{
    public class WallpaperDetailViewModel : ViewModelBase
    {
        private readonly IWallpaperService _wallpaperService;
        private readonly IUserService _userService;
        private readonly IUserPointService _pointService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly UserEntity _currentUser;

        public WallpaperDetailViewModel(
            IWallpaperService wallpaperService,
            IUserService userService,
            IUserPointService pointService,
            IDialogCoordinator dialogCoordinator,
            UserEntity currentUser,
            WallpaperEntity wallpaper)
        {
            _wallpaperService = wallpaperService;
            _userService = userService;
            _pointService = pointService;
            _dialogCoordinator = dialogCoordinator;
            _currentUser = currentUser;

            Title = "壁纸详情";
            Wallpaper = wallpaper;

            // 增加视图计数
            _wallpaperService.IncrementViewCountAsync(wallpaper.Id).Subscribe();

            // 加载壁纸图片
            LoadWallpaperImage();

            // 初始化命令
            DownloadCommand = ReactiveCommand.CreateFromTask(DownloadWallpaperAsync);
            FavoriteCommand = ReactiveCommand.CreateFromTask(FavoriteWallpaperAsync);
            ReportCommand = ReactiveCommand.CreateFromTask(ReportWallpaperAsync);
        }

        #region 属性

        private WallpaperEntity _wallpaper;
        public WallpaperEntity Wallpaper
        {
            get => _wallpaper;
            set => this.RaiseAndSetIfChanged(ref _wallpaper, value);
        }

        private BitmapImage _wallpaperImage;
        public BitmapImage WallpaperImage
        {
            get => _wallpaperImage;
            set => this.RaiseAndSetIfChanged(ref _wallpaperImage, value);
        }

        private bool _isFavorite;
        public bool IsFavorite
        {
            get => _isFavorite;
            set => this.RaiseAndSetIfChanged(ref _isFavorite, value);
        }

        #endregion

        #region 命令

        public ReactiveCommand<Unit, Unit> DownloadCommand { get; }
        public ReactiveCommand<Unit, Unit> FavoriteCommand { get; }
        public ReactiveCommand<Unit, Unit> ReportCommand { get; }

        #endregion

        #region 方法

        private async void LoadWallpaperImage()
        {
            try
            {
                using var stream = await _wallpaperService.GetPreviewStreamAsync(Wallpaper.Id);

                if (stream != null)
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    WallpaperImage = bitmap;
                }
            }
            catch (Exception ex)
            {
                ShowError($"加载壁纸图片失败: {ex.Message}");
            }
        }

        private async Task DownloadWallpaperAsync()
        {
            if (_currentUser == null)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "提示", "请先登录才能下载壁纸");
                return;
            }

            // 检查用户积分是否足够
            var userPoints = await _pointService.GetUserPointsAsync(_currentUser.Id);

            // 假设下载需要5积分
            const int downloadPoints = 5;

            if (userPoints < downloadPoints)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "积分不足",
                    $"下载此壁纸需要 {downloadPoints} 积分，您当前只有 {userPoints} 积分");
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "正在准备下载...";

                // 扣除积分
                var pointsDeducted = await _pointService.DeductPointsAsync(
                    _currentUser.Id,
                    downloadPoints,
                    $"下载壁纸《{Wallpaper.Title}》消耗{downloadPoints}积分");

                if (!pointsDeducted)
                {
                    await _dialogCoordinator.ShowMessageAsync(this, "下载失败", "积分扣除失败");
                    return;
                }

                // 增加下载计数
                await _wallpaperService.IncrementDownloadCountAsync(Wallpaper.Id);

                // 保存文件对话框
                var saveFileDialog = new SaveFileDialog
                {
                    FileName = Wallpaper.FileName,
                    Filter = "图像文件|*.jpg;*.jpeg;*.png;*.bmp;*.webp"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 下载文件
                    using var sourceStream = await _wallpaperService.GetWallpaperStreamAsync(Wallpaper.Id);

                    if (sourceStream == null)
                    {
                        await _dialogCoordinator.ShowMessageAsync(this, "下载失败", "壁纸文件不存在");
                        return;
                    }

                    using var destinationStream = File.Create(saveFileDialog.FileName);
                    await sourceStream.CopyToAsync(destinationStream);

                    StatusMessage = "下载完成";
                    await _dialogCoordinator.ShowMessageAsync(this, "下载成功",
                        $"壁纸《{Wallpaper.Title}》已成功下载到 {saveFileDialog.FileName}");
                }
                else
                {
                    // 用户取消了下载，退还积分
                    await _pointService.AddPointsAsync(
                        _currentUser.Id,
                        downloadPoints,
                        PointAction.DownloadWallpaper,
                        "下载取消，积分已退还");
                }
            }
            catch (Exception ex)
            {
                ShowError($"下载壁纸失败: {ex.Message}");

                // 发生错误时退还积分
                await _pointService.AddPointsAsync(
                    _currentUser.Id,
                    downloadPoints,
                    PointAction.DownloadWallpaper,
                    "下载失败，积分已退还");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task FavoriteWallpaperAsync()
        {
            if (_currentUser == null)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "提示", "请先登录才能收藏壁纸");
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "正在更新收藏状态...";

                // 在实际应用中，这里应该调用收藏服务
                // 简化示例，仅切换状态
                IsFavorite = !IsFavorite;

                StatusMessage = IsFavorite ? "已收藏此壁纸" : "已取消收藏";
                await _dialogCoordinator.ShowMessageAsync(this, "成功",
                    IsFavorite ? "已成功收藏此壁纸" : "已取消收藏此壁纸");
            }
            catch (Exception ex)
            {
                ShowError($"操作失败: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ReportWallpaperAsync()
        {
            if (_currentUser == null)
            {
                await _dialogCoordinator.ShowMessageAsync(this, "提示", "请先登录才能举报壁纸");
                return;
            }

            try
            {
                // 获取举报原因
                var result = await _dialogCoordinator.ShowInputAsync(
                    this,
                    "举报壁纸",
                    "请输入举报原因:");

                if (string.IsNullOrWhiteSpace(result))
                {
                    return; // 用户取消了举报
                }

                IsLoading = true;
                StatusMessage = "正在提交举报...";

                // 在实际应用中，这里应该调用举报服务
                // 简化示例，仅显示成功消息

                StatusMessage = "举报已提交";
                await _dialogCoordinator.ShowMessageAsync(this, "举报成功",
                    "感谢您的举报，我们将尽快审核并处理此壁纸");
            }
            catch (Exception ex)
            {
                ShowError($"举报失败: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion
    }
}