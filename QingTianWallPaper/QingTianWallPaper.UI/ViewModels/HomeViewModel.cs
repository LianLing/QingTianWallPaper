// QingTianWallPaper.UI/ViewModels/HomeViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using QingTianWallPaper.Core.Models;
using QingTianWallPaper.Core.Services.Interfaces;
using ReactiveUI;

namespace QingTianWallPaper.UI.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly IWallpaperService _wallpaperService;
        private readonly IDialogCoordinator _dialogCoordinator;

        public HomeViewModel(IWallpaperService wallpaperService, IDialogCoordinator dialogCoordinator)
        {
            _wallpaperService = wallpaperService;
            _dialogCoordinator = dialogCoordinator;

            // 初始化命令
            LoadWallpapersCommand = ReactiveCommand.CreateFromTask(LoadWallpapersAsync);
            SearchCommand = ReactiveCommand.CreateFromTask(SearchWallpapersAsync);
            ApplyFilterCommand = ReactiveCommand.Create(ApplyFilter);

            // 初始化属性
            Wallpapers = new ObservableCollection<Wallpaper>();
            Categories = new ObservableCollection<string> { "全部", "风景", "动物", "抽象", "人物", "动漫" };
            SelectedCategory = "全部";

            // 加载壁纸
            LoadWallpapersCommand.Execute().Subscribe();
        }

        #region 属性

        private ObservableCollection<Wallpaper> _wallpapers;
        public ObservableCollection<Wallpaper> Wallpapers
        {
            get => _wallpapers;
            set => this.RaiseAndSetIfChanged(ref _wallpapers, value);
        }

        private ObservableCollection<string> _categories;
        public ObservableCollection<string> Categories
        {
            get => _categories;
            set => this.RaiseAndSetIfChanged(ref _categories, value);
        }

        private string _selectedCategory;
        public string SelectedCategory
        {
            get => _selectedCategory;
            set => this.RaiseAndSetIfChanged(ref _selectedCategory, value);
        }

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        #endregion

        #region 命令

        public ReactiveCommand<Unit, Unit> LoadWallpapersCommand { get; }
        public ReactiveCommand<Unit, Unit> SearchCommand { get; }
        public ReactiveCommand<Unit, Unit> ApplyFilterCommand { get; }

        #endregion

        #region 方法

        private async Task LoadWallpapersAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "正在加载壁纸...";

                var wallpapers = await _wallpaperService.GetApprovedWallpapersAsync();

                Wallpapers.Clear();
                foreach (var wallpaper in wallpapers)
                {
                    Wallpapers.Add(wallpaper);
                }

                StatusMessage = $"已加载 {Wallpapers.Count} 张壁纸";
            }
            catch (Exception ex)
            {
                StatusMessage = "加载壁纸失败";
                await _dialogCoordinator.ShowMessageAsync(this, "错误", $"获取壁纸时出错: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchWallpapersAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                await LoadWallpapersAsync();
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = $"搜索: {SearchQuery}";

                var wallpapers = await _wallpaperService.SearchWallpapersAsync(SearchQuery);

                Wallpapers.Clear();
                foreach (var wallpaper in wallpapers)
                {
                    Wallpapers.Add(wallpaper);
                }

                StatusMessage = $"找到 {Wallpapers.Count} 个结果";
            }
            catch (Exception ex)
            {
                StatusMessage = "搜索失败";
                await _dialogCoordinator.ShowMessageAsync(this, "错误", $"搜索壁纸时出错: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyFilter()
        {
            if (SelectedCategory == "全部")
            {
                LoadWallpapersCommand.Execute().Subscribe();
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = $"筛选: {SelectedCategory}";

                var filtered = Wallpapers.Where(w =>
                    w.Type.ToString() == SelectedCategory ||
                    w.Description.Contains(SelectedCategory, StringComparison.OrdinalIgnoreCase)).ToList();

                var temp = new ObservableCollection<Wallpaper>(filtered);
                Wallpapers = temp;

                StatusMessage = $"找到 {Wallpapers.Count} 个结果";
            }
            catch (Exception ex)
            {
                StatusMessage = "筛选失败";
                _dialogCoordinator.ShowMessageAsync(this, "错误", $"筛选壁纸时出错: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion
    }
}