// QingTianWallPaper.UI/ViewModels/UserProfileViewModel.cs
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using QingTianWallPaper.Core.Models;
using QingTianWallPaper.Core.Services.Interfaces;
using ReactiveUI;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QingTianWallPaper.UI.ViewModels
{
    public class UserProfileViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly UserEntity _currentUser;

        public UserProfileViewModel(
            IUserService userService,
            IDialogCoordinator dialogCoordinator,
            UserEntity currentUser)
        {
            _userService = userService;
            _dialogCoordinator = dialogCoordinator;
            _currentUser = currentUser;

            Title = "个人资料";

            // 初始化用户数据
            Username = _currentUser.Username;
            Email = _currentUser.Email;
            Bio = _currentUser.Bio;
            AvatarUrl = _currentUser.AvatarUrl;
            Points = _currentUser.Points;
            IsPremium = _currentUser.IsPremium;

            // 初始化命令
            SaveCommand = ReactiveCommand.CreateFromTask(SaveChangesAsync);
            ChangePasswordCommand = ReactiveCommand.CreateFromTask(ChangePasswordAsync);
            UploadAvatarCommand = ReactiveCommand.Create(UploadAvatar);
        }

        #region 属性

        private string _username;
        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => this.RaiseAndSetIfChanged(ref _email, value);
        }

        private string _bio;
        public string Bio
        {
            get => _bio;
            set => this.RaiseAndSetIfChanged(ref _bio, value);
        }

        private string _avatarUrl;
        public string AvatarUrl
        {
            get => _avatarUrl;
            set => this.RaiseAndSetIfChanged(ref _avatarUrl, value);
        }

        private int _points;
        public int Points
        {
            get => _points;
            set => this.RaiseAndSetIfChanged(ref _points, value);
        }

        private bool _isPremium;
        public bool IsPremium
        {
            get => _isPremium;
            set => this.RaiseAndSetIfChanged(ref _isPremium, value);
        }

        #endregion

        #region 命令

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> ChangePasswordCommand { get; }
        public ReactiveCommand<Unit, Unit> UploadAvatarCommand { get; }

        #endregion

        #region 方法

        private async Task SaveChangesAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "正在保存更改...";

                // 更新用户信息
                _currentUser.Username = Username;
                _currentUser.Email = Email;
                _currentUser.Bio = Bio;
                _currentUser.AvatarUrl = AvatarUrl;

                await _userService.UpdateUserAsync(_currentUser);

                StatusMessage = "个人资料已更新";
                await _dialogCoordinator.ShowMessageAsync(this, "成功", "个人资料已成功更新");
            }
            catch (Exception ex)
            {
                ShowError($"保存更改失败: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ChangePasswordAsync()
        {
            try
            {
                // 创建密码更改对话框
                var metroDialogSettings = new MetroDialogSettings
                {
                    AffirmativeButtonText = "更改",
                    NegativeButtonText = "取消",
                    AnimateShow = true,
                    AnimateHide = false
                };

                var dialog = new CustomDialog(metroDialogSettings);
                dialog.Title = "更改密码";

                // 这里应该有一个自定义的密码更改控件
                // 简化示例，实际项目中应使用自定义控件
                var passwordDialog = new PasswordChangeDialog();
                dialog.Content = passwordDialog;

                await _dialogCoordinator.ShowMetroDialogAsync(this, dialog);

                var result = await passwordDialog.WaitForButtonPressAsync();

                if (result == PasswordDialogResult.OK)
                {
                    var oldPassword = passwordDialog.OldPassword;
                    var newPassword = passwordDialog.NewPassword;
                    var confirmPassword = passwordDialog.ConfirmPassword;

                    if (string.IsNullOrWhiteSpace(oldPassword) ||
                        string.IsNullOrWhiteSpace(newPassword) ||
                        string.IsNullOrWhiteSpace(confirmPassword))
                    {
                        await _dialogCoordinator.ShowMessageAsync(this, "错误", "所有密码字段均为必填项");
                        return;
                    }

                    if (newPassword != confirmPassword)
                    {
                        await _dialogCoordinator.ShowMessageAsync(this, "错误", "新密码和确认密码不匹配");
                        return;
                    }

                    IsLoading = true;
                    StatusMessage = "正在更改密码...";

                    var success = await _userService.ChangePasswordAsync(_currentUser.Id, oldPassword, newPassword);

                    if (success)
                    {
                        StatusMessage = "密码已更改";
                        await _dialogCoordinator.ShowMessageAsync(this, "成功", "密码已成功更改");
                    }
                    else
                    {
                        await _dialogCoordinator.ShowMessageAsync(this, "失败", "旧密码不正确");
                    }
                }

                await _dialogCoordinator.HideMetroDialogAsync(this, dialog);
            }
            catch (Exception ex)
            {
                ShowError($"更改密码失败: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UploadAvatar()
        {
            try
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "图像文件|*.jpg;*.jpeg;*.png;*.bmp";
                    openFileDialog.Multiselect = false;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // 实际项目中应上传到服务器并获取URL
                        // 此处简化为直接使用本地文件路径
                        AvatarUrl = openFileDialog.FileName;

                        StatusMessage = "头像已更新";
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"上传头像失败: {ex.Message}");
            }
        }

        #endregion
    }
}