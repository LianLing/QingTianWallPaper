// QingTianWallPaper.UI/ViewModels/UserPointViewModel.cs
using MahApps.Metro.Controls.Dialogs;
using QingTianWallPaper.Core.Models;
using QingTianWallPaper.Core.Services.Interfaces;
using QingTianWallPaper.QingTianWallPaper.Core.Services.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace QingTianWallPaper.UI.ViewModels
{
    public class UserPointViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly IUserPointService _pointService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly User _currentUser;

        public UserPointViewModel(
            IUserService userService,
            IUserPointService pointService,
            IDialogCoordinator dialogCoordinator,
            User currentUser)
        {
            _userService = userService;
            _pointService = pointService;
            _dialogCoordinator = dialogCoordinator;
            _currentUser = currentUser;

            Title = "我的积分";
            PointHistory = new ObservableCollection<UserPoint>();
            CurrentPage = 1;
            PageSize = 10;

            // 初始化命令
            LoadPointsCommand = ReactiveCommand.CreateFromTask(LoadPointsAsync);
            NextPageCommand = ReactiveCommand.Create(NextPage, this.WhenAnyValue(x => x.CanGoNextPage));
            PreviousPageCommand = ReactiveCommand.Create(PreviousPage, this.WhenAnyValue(x => x.CanGoPreviousPage));

            // 初始加载
            LoadPointsCommand.Execute().Subscribe();
        }

        #region 属性

        private int _currentPoints;
        public int CurrentPoints
        {
            get => _currentPoints;
            set => this.RaiseAndSetIfChanged(ref _currentPoints, value);
        }

        private ObservableCollection<UserPoint> _pointHistory;
        public ObservableCollection<UserPoint> PointHistory
        {
            get => _pointHistory;
            set => this.RaiseAndSetIfChanged(ref _pointHistory, value);
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

        public bool CanGoNextPage => CurrentPage < TotalPages;
        public bool CanGoPreviousPage => CurrentPage > 1;

        #endregion

        #region 命令

        public ReactiveCommand<Unit, Unit> LoadPointsCommand { get; }
        public ReactiveCommand<Unit, Unit> NextPageCommand { get; }
        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; }

        #endregion

        #region 方法

        private async Task LoadPointsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "正在加载积分信息...";

                // 获取当前用户积分
                CurrentPoints = await _pointService.GetUserPointsAsync(_currentUser.Id);

                // 获取积分历史
                var history = await _pointService.GetUserPointHistoryAsync(_currentUser.Id, CurrentPage, PageSize);

                PointHistory.Clear();
                foreach (var item in history)
                {
                    PointHistory.Add(item);
                }

                // 计算总页数 (假设我们知道总记录数)
                var totalCount = 50; // 实际项目中应该从服务获取
                TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                StatusMessage = $"积分信息加载完成";
            }
            catch (Exception ex)
            {
                ShowError($"加载积分信息失败: {ex.Message}");
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
                LoadPointsCommand.Execute().Subscribe();
            }
        }

        private void PreviousPage()
        {
            if (CanGoPreviousPage)
            {
                CurrentPage--;
                LoadPointsCommand.Execute().Subscribe();
            }
        }

        #endregion
    }
}