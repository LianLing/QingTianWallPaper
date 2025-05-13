// QingTianWallPaper.UI/ViewModels/BrowseViewModel.cs
using MahApps.Metro.Controls.Dialogs;
using QingTianWallPaper.Core.Models;
using QingTianWallPaper.Core.Services.Interfaces;
using QingTianWallPaper.QingTianWallPaper.Core.Services.Interfaces;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QingTianWallPaper.UI.ViewModels
{
    public class BrowseViewModel : ViewModelBase
    {
        private readonly IWallpaperService _wallpaperService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IUserService _userService;

        public BrowseViewModel(
            IWallpaperService wallpaperService,
            IDialogCoordinator dialogCoordinator,
            IUserService userService)
        {
            _wallpaperService = wallpaperService;
            _dialogCoordinator = dialogCoordinator;
            _userService = userService;

            Title = "壁纸浏览";
            CurrentPage = 1;
            PageSize = 24;
            AvailableResolutions = new ObservableCollection<string>
            {
                "全部", "1920x1080", "2560x1440", "3840x2160", "1366x768", "其他"
            };
            SelectedResolution = "全部";

            // 初始化命令
            LoadWallpapersCommand = ReactiveCommand.CreateFromTask(LoadWallpapersAsync);
            NextPageCommand = ReactiveCommand.Create(NextPage, this.WhenAnyValue(x => x.CanGoNextPage));
            PreviousPageCommand = ReactiveCommand.Create(PreviousPage, this.WhenAnyValue(x => x.CanGoPreviousPage));
            DownloadCommand = ReactiveCommand.CreateFromTask<Wallpaper>(DownloadWallpaperAsync);
            SetAsWallpaperCommand = ReactiveCommand.CreateFromTask<Wallpaper>(SetAsWallpaperAsync);
            LikeCommand = ReactiveCommand.CreateFromTask<Wallpaper>(LikeWallpaperAsync);
            FilterCommand = ReactiveCommand.CreateFromTask(ApplyFilterAsync);

            // 初始加载
            LoadWallpapersCommand.Execute().Subscribe();
        }

        #region 属性

        private ObservableCollection<Wallpaper> _wallpapers = new();
        public ObservableCollection<Wallpaper> Wallpapers
        {
            get => _wallpapers;
            set => this.RaiseAndSetIfChanged(ref _wallpapers, value);
        }

        private ObservableCollection<string> _categories = new();
        public ObservableCollection<string> Categories
        {
            get => _categories;
            set => this.RaiseAndSetIfChanged(ref _categories, value);
        }

        private string _selectedCategory = "全部";
        public string SelectedCategory
        {
            get => _selectedCategory;
            set => this.RaiseAndSetIfChanged(ref _selectedCategory, value);
        }

        private ObservableCollection<string> _availableResolutions = new();
        public ObservableCollection<string> AvailableResolutions
        {
            get => _availableResolutions;
            set => this.RaiseAndSetIfChanged(ref _availableResolutions, value);
        }

        private string _selectedResolution = "全部";
        public string SelectedResolution
        {
            get => _selectedResolution;
            set => this.RaiseAndSetIfChanged(ref _selectedResolution, value);
        }

        private int _currentPage;
        public int CurrentPage
        {
            get => _currentPage;
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }

        private int _pageSize;
        public int PageSize
        {
            get => _pageSize;
            set => this.RaiseAndSetIfChanged(ref _pageSize, value);
        }

        private int _totalPages;
        public int TotalPages
        {
            get => _totalPages;
            set => this.RaiseAndSetIfChanged(ref _totalPages, value);
        }

        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set => this.RaiseAndSetIfChanged(ref _totalCount, value);
        }

        public bool CanGoNextPage => CurrentPage < TotalPages;
        public bool CanGoPreviousPage => CurrentPage > 1;

        private Wallpaper _selectedWallpaper;
        public Wallpaper SelectedWallpaper
        {
            get => _selectedWallpaper;
            set => this.RaiseAndSetIfChanged(ref _selectedWallpaper, value);
        }

        #endregion

        #region 命令

        public ReactiveCommand<Unit, Unit> LoadWallpapersCommand { get; }
        public ReactiveCommand<Unit, Unit> NextPageCommand { get; }
        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; }
        public ReactiveCommand<Wallpaper, Unit> DownloadCommand { get; }
        public ReactiveCommand<Wallpaper, Unit> SetAsWallpaperCommand { get; }
        public ReactiveCommand<Wallpaper, Unit> LikeCommand { get; }
        public ReactiveCommand<Unit, Unit> FilterCommand { get; }

        #endregion

        #region 方法

        private async Task LoadWallpapersAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = $"加载第 {CurrentPage} 页壁纸...";

                var filter = new WallpaperFilter
                {
                    Category = SelectedCategory == "全部" ? null : SelectedCategory,
                    Resolution = SelectedResolution == "全部" ? null : SelectedResolution,
                    Page = CurrentPage,
                    PageSize = PageSize
                };

                var result = await _wallpaperService.GetFilteredWallpapersAsync(filter);
                Wallpapers.Clear();

                foreach (var wallpaper in result.Items)
                {
                    Wallpapers.Add(wallpaper);
                }

                TotalCount = result.TotalCount;
                TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);

                StatusMessage = $"已加载 {Wallpapers.Count} 张壁纸 (共 {TotalCount} 张)";
            }
            catch (Exception ex)
            {
                ShowError($"加载壁纸失败: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void NextPage()
        {
            if (CanGoNextPage)
            {
                CurrentPage++;
                LoadWallpapersCommand.Execute().Subscribe();
            }
        }

        private void PreviousPage()
        {
            if (CanGoPreviousPage)
            {
                CurrentPage--;
                LoadWallpapersCommand.Execute().Subscribe();
            }
        }

        private async Task DownloadWallpaperAsync(Wallpaper wallpaper)
        {
            try
            {
                IsLoading = true;
                StatusMessage = $"正在下载: {wallpaper.Title}...";

                // 模拟下载逻辑
                await Task.Delay(1000);

                // 实际项目中应该实现文件下载
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = Path.GetFileName(wallpaper.FilePath),
                    Filter = "图像文件|*.jpg;*.png;*.bmp;*.gif;*.webp"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // 复制文件 (实际项目中可能需要从网络下载)
                    File.Copy(wallpaper.FilePath, saveDialog.FileName, true);

                    // 更新下载计数
                    wallpaper.DownloadCount++;
                    await _wallpaperService.UpdateWallpaperAsync(wallpaper);

                    await _dialogCoordinator.ShowMessageAsync(this, "下载成功",
                        $"壁纸已保存到: {saveDialog.FileName}");

                    StatusMessage = $"下载完成: {wallpaper.Title}";
                }
                else
                {
                    StatusMessage = "下载已取消";
                }
            }
            catch (Exception ex)
            {
                ShowError($"下载壁纸失败: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SetAsWallpaperAsync(Wallpaper wallpaper)
        {
            try
            {
                IsLoading = true;
                StatusMessage = $"正在设置壁纸: {wallpaper.Title}...";

                // 实际项目中应该实现设置桌面壁纸的逻辑
                // 例如使用 SystemParametersInfo API
                await Task.Delay(500);

                // 模拟设置成功
                await _dialogCoordinator.ShowMessageAsync(this, "设置成功",
                    $"桌面壁纸已设置为: {wallpaper.Title}");

                StatusMessage = $"壁纸设置成功";
            }
            catch (Exception ex)
            {
                ShowError($"设置壁纸失败: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LikeWallpaperAsync(Wallpaper wallpaper)
        {
            try
            {
                // 检查用户是否已登录 (实际项目中应实现身份验证)
                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    await _dialogCoordinator.ShowMessageAsync(this, "请登录",
                        "请先登录以使用收藏功能");
                    return;
                }

                // 模拟点赞逻辑
                wallpaper.LikesCount++;
                await _wallpaperService.UpdateWallpaperAsync(wallpaper);

                StatusMessage = $"已点赞: {wallpaper.Title}";
            }
            catch (Exception ex)
            {
                ShowError($"操作失败: {ex.Message}");
            }
        }

        private async Task ApplyFilterAsync()
        {
            CurrentPage = 1; // 重置到第一页
            await LoadWallpapersAsync();
        }

        #endregion
    }

    // 壁纸筛选模型
    public class WallpaperFilter
    {
        public string? Category { get; set; }
        public string? Resolution { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}