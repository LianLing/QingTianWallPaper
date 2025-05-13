// QingTianWallPaper.UI/ViewModels/ReviewViewModel.cs
using MahApps.Metro.Controls.Dialogs;
using QingTianWallPaper.Core.Enums;
using QingTianWallPaper.Core.Models;
using QingTianWallPaper.Core.Services.Interfaces;
using QingTianWallPaper.QingTianWallPaper.Core.Services.Interfaces;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace QingTianWallPaper.UI.ViewModels
{
    public class WallpaperReviewViewModel : ViewModelBase
    {
        private readonly IWallpaperService _wallpaperService;
        private readonly IUserService _userService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly User _currentUser;

        public WallpaperReviewViewModel(
            IWallpaperService wallpaperService,
            IUserService userService,
            IDialogCoordinator dialogCoordinator,
            User currentUser,
            IEnumerable<Wallpaper> wallpapersToReview)
        {
            _wallpaperService = wallpaperService;
            _userService = userService;
            _dialogCoordinator = dialogCoordinator;
            _currentUser = currentUser;

            Title = "壁纸审核";
            PendingWallpapers = new ObservableCollection<Wallpaper>(wallpapersToReview);
            CurrentWallpaperIndex = 0;

            // 初始化命令
            ApproveCommand = ReactiveCommand.CreateFromTask(ApproveWallpaperAsync);
            RejectCommand = ReactiveCommand.CreateFromTask(RejectWallpaperAsync);
            NextWallpaperCommand = ReactiveCommand.Create(NextWallpaper, this.WhenAnyValue(x => x.CanGoNext));
            PreviousWallpaperCommand = ReactiveCommand.Create(PreviousWallpaper, this.WhenAnyValue(x => x.CanGoPrevious));

            // 初始化当前壁纸
            UpdateCurrentWallpaper();
        }

        #region 属性

        private ObservableCollection<Wallpaper> _pendingWallpapers;
        public ObservableCollection<Wallpaper> PendingWallpapers
        {
            get => _pendingWallpapers;
            set => this.RaiseAndSetIfChanged(ref _pendingWallpapers, value);
        }

        private int _currentWallpaperIndex;
        public int CurrentWallpaperIndex
        {
            get => _currentWallpaperIndex;
            set => this.RaiseAndSetIfChanged(ref _currentWallpaperIndex, value);
        }

        private Wallpaper _currentWallpaper;
        public Wallpaper CurrentWallpaper
        {
            get => _currentWallpaper;
            set => this.RaiseAndSetIfChanged(ref _currentWallpaper, value);
        }

        private string _reviewComment;
        public string ReviewComment
        {
            get => _reviewComment;
            set => this.RaiseAndSetIfChanged(ref _reviewComment, value);
        }

        public bool CanGoNext => PendingWallpapers.Any() && CurrentWallpaperIndex < PendingWallpapers.Count - 1;
        public bool CanGoPrevious => PendingWallpapers.Any() && CurrentWallpaperIndex > 0;

        public int PendingCount => PendingWallpapers.Count;

        #endregion

        #region 命令

        public ReactiveCommand<Unit, Unit> ApproveCommand { get; }
        public ReactiveCommand<Unit, Unit> RejectCommand { get; }
        public ReactiveCommand<Unit, Unit> NextWallpaperCommand { get; }
        public ReactiveCommand<Unit, Unit> PreviousWallpaperCommand { get; }

        #endregion

        #region 方法

        private void UpdateCurrentWallpaper()
        {
            if (PendingWallpapers.Any())
            {
                CurrentWallpaper = PendingWallpapers[CurrentWallpaperIndex];
                StatusMessage = $"审核壁纸 {CurrentWallpaperIndex + 1}/{PendingWallpapers.Count}";
            }
            else
            {
                CurrentWallpaper = null;
                StatusMessage = "没有待审核的壁纸";
            }
        }

        private void NextWallpaper()
        {
            if (CanGoNext)
            {
                CurrentWallpaperIndex++;
                UpdateCurrentWallpaper();
            }
        }

        private void PreviousWallpaper()
        {
            if (CanGoPrevious)
            {
                CurrentWallpaperIndex--;
                UpdateCurrentWallpaper();
            }
        }

        private async Task ApproveWallpaperAsync()
        {
            if (CurrentWallpaper == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "正在提交审核...";

                // 更新壁纸状态
                CurrentWallpaper.ReviewStatus = ReviewStatus.Approved;
                CurrentWallpaper.ReviewTime = DateTime.Now;
                CurrentWallpaper.ReviewerId = _currentUser.Id;
                CurrentWallpaper.ReviewComment = ReviewComment;

                // 保存审核结果
                await _wallpaperService.UpdateWallpaperAsync(CurrentWallpaper);

                // 为上传者增加积分
                await _userService.AddPointsAsync(CurrentWallpaper.UploaderId, 10);

                // 从待审核列表移除
                PendingWallpapers.Remove(CurrentWallpaper);

                // 如果还有待审核的壁纸，移动到下一个
                if (PendingWallpapers.Any())
                {
                    if (CurrentWallpaperIndex >= PendingWallpapers.Count)
                        CurrentWallpaperIndex = PendingWallpapers.Count - 1;

                    UpdateCurrentWallpaper();
                    ReviewComment = string.Empty;

                    await _dialogCoordinator.ShowMessageAsync(this, "审核完成",
                        "壁纸已通过审核，上传者已获得10积分");
                }
                else
                {
                    StatusMessage = "所有壁纸已审核完毕";
                    await _dialogCoordinator.ShowMessageAsync(this, "审核完成",
                        "恭喜！您已完成所有待审核的壁纸");
                }
            }
            catch (Exception ex)
            {
                ShowError($"审核失败: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RejectWallpaperAsync()
        {
            if (CurrentWallpaper == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "正在提交审核...";

                // 更新壁纸状态
                CurrentWallpaper.ReviewStatus = ReviewStatus.Rejected;
                CurrentWallpaper.ReviewTime = DateTime.Now;
                CurrentWallpaper.ReviewerId = _currentUser.Id;
                CurrentWallpaper.ReviewComment = ReviewComment;

                // 保存审核结果
                await _wallpaperService.UpdateWallpaperAsync(CurrentWallpaper);

                // 从待审核列表移除
                PendingWallpapers.Remove(CurrentWallpaper);

                // 如果还有待审核的壁纸，移动到下一个
                if (PendingWallpapers.Any())
                {
                    if (CurrentWallpaperIndex >= PendingWallpapers.Count)
                        CurrentWallpaperIndex = PendingWallpapers.Count - 1;

                    UpdateCurrentWallpaper();
                    ReviewComment = string.Empty;

                    await _dialogCoordinator.ShowMessageAsync(this, "审核完成",
                        "壁纸已被拒绝");
                }
                else
                {
                    StatusMessage = "所有壁纸已审核完毕";
                    await _dialogCoordinator.ShowMessageAsync(this, "审核完成",
                        "恭喜！您已完成所有待审核的壁纸");
                }
            }
            catch (Exception ex)
            {
                ShowError($"审核失败: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion
    }
}