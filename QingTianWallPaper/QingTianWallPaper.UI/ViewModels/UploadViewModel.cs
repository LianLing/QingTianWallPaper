// QingTianWallPaper.UI/ViewModels/UploadViewModel.cs
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using QingTianWallPaper.Core.Models;
using QingTianWallPaper.Core.Services.Interfaces;
using ReactiveUI;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;

namespace QingTianWallPaper.UI.ViewModels
{
    public class UploadViewModel : ViewModelBase
    {
        private readonly IWallpaperService _wallpaperService;
        private readonly IUserService _userService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly User _currentUser;

        public UploadViewModel(
            IWallpaperService wallpaperService,
            IUserService userService,
            IDialogCoordinator dialogCoordinator,
            User currentUser)
        {
            _wallpaperService = wallpaperService;
            _userService = userService;
            _dialogCoordinator = dialogCoordinator;
            _currentUser = currentUser;

            Title = "上传壁纸";
            AvailableCategories = new List<string>
            {
                "风景", "动物", "人物", "抽象", "动漫", "城市", "自然", "科技"
            };
            AvailableResolutions = new List<string>
            {
                "1920x1080", "2560x1440", "3840x2160", "1366x768", "其他"
            };

            // 初始化命令
            SelectFileCommand = ReactiveCommand.Create(SelectFile);
            UploadCommand = ReactiveCommand.CreateFromTask(UploadWallpaperAsync,
                this.WhenAnyValue(
                    x => x.Title,
                    x => x.Description,
                    x => x.SelectedCategory,
                    x => x.SelectedResolution,
                    x => x.FilePath,
                    (title, desc, category, resolution, filePath) =>
                        !string.IsNullOrWhiteSpace(title) &&
                        !string.IsNullOrWhiteSpace(desc) &&
                        !string.IsNullOrWhiteSpace(category) &&
                        !string.IsNullOrWhiteSpace(resolution) &&
                        !string.IsNullOrWhiteSpace(filePath)));
        }

        #region 属性

        private string _title;
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        private List<string> _availableCategories;
        public List<string> AvailableCategories
        {
            get => _availableCategories;
            set => this.RaiseAndSetIfChanged(ref _availableCategories, value);
        }

        private string _selectedCategory;
        public string SelectedCategory
        {
            get => _selectedCategory;
            set => this.RaiseAndSetIfChanged(ref _selectedCategory, value);
        }

        private List<string> _availableResolutions;
        public List<string> AvailableResolutions
        {
            get => _availableResolutions;
            set => this.RaiseAndSetIfChanged(ref _availableResolutions, value);
        }

        private string _selectedResolution;
        public string SelectedResolution
        {
            get => _selectedResolution;
            set => this.RaiseAndSetIfChanged(ref _selectedResolution, value);
        }

        private string _filePath;
        public string FilePath
        {
            get => _filePath;
            set => this.RaiseAndSetIfChanged(ref _filePath, value);
        }

        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set => this.RaiseAndSetIfChanged(ref _fileName, value);
        }

        private long _fileSize;
        public long FileSize
        {
            get => _fileSize;
            set => this.RaiseAndSetIfChanged(ref _fileSize, value);
        }

        private string _fileSizeText;
        public string FileSizeText
        {
            get => _fileSizeText;
            set => this.RaiseAndSetIfChanged(ref _fileSizeText, value);
        }

        private byte[] _thumbnailData;
        public byte[] ThumbnailData
        {
            get => _thumbnailData;
            set => this.RaiseAndSetIfChanged(ref _thumbnailData, value);
        }

        private double _uploadProgress;
        public double UploadProgress
        {
            get => _uploadProgress;
            set => this.RaiseAndSetIfChanged(ref _uploadProgress, value);
        }

        private bool _isUploading;
        public bool IsUploading
        {
            get => _isUploading;
            set => this.RaiseAndSetIfChanged(ref _isUploading, value);
        }

        #endregion

        #region 命令

        public ReactiveCommand<Unit, Unit> SelectFileCommand { get; }
        public ReactiveCommand<Unit, Unit> UploadCommand { get; }

        #endregion

        #region 方法

        private void SelectFile()
        {
            try
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "图像文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp";
                    openFileDialog.Multiselect = false;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        FilePath = openFileDialog.FileName;
                        FileName = Path.GetFileName(openFileDialog.FileName);
                        FileSize = new FileInfo(openFileDialog.FileName).Length;
                        FileSizeText = FormatFileSize(FileSize);

                        // 创建缩略图
                        CreateThumbnail(openFileDialog.FileName);

                        StatusMessage = $"已选择文件: {FileName}";
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"选择文件时出错: {ex.Message}");
            }
        }

        private void CreateThumbnail(string filePath)
        {
            try
            {
                using (var image = System.Drawing.Image.FromFile(filePath))
                {
                    int maxWidth = 300;
                    int maxHeight = 200;
                    double ratioX = (double)maxWidth / image.Width;
                    double ratioY = (double)maxHeight / image.Height;
                    double ratio = Math.Min(ratioX, ratioY);

                    int newWidth = (int)(image.Width * ratio);
                    int newHeight = (int)(image.Height * ratio);

                    using (var thumbnail = image.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero))
                    using (var ms = new MemoryStream())
                    {
                        thumbnail.Save(ms, image.RawFormat);
                        ThumbnailData = ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"创建缩略图时出错: {ex.Message}");
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < suffixes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {suffixes[order]}";
        }

        private async Task UploadWallpaperAsync()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                await _dialogCoordinator.ShowMessageAsync(this, "上传失败", "请先选择要上传的文件");
                return;
            }

            try
            {
                IsUploading = true;
                UploadProgress = 0;
                StatusMessage = "正在上传壁纸...";

                // 创建壁纸对象
                var wallpaper = new Wallpaper
                {
                    Title = Title,
                    Description = Description,
                    Type = (WallpaperType)Enum.Parse(typeof(WallpaperType), SelectedCategory),
                    Resolution = SelectedResolution,
                    FileName = FileName,
                    FileSize = FileSize,
                    UploaderId = _currentUser.Id,
                    UploadTime = DateTime.Now,
                    ReviewStatus = ReviewStatus.Pending,
                    ThumbnailPath = $"thumbnails/{Guid.NewGuid()}.jpg" // 实际项目中应生成缩略图并保存
                };

                // 模拟上传进度
                for (int i = 0; i <= 100; i += 10)
                {
                    UploadProgress = i;
                    await Task.Delay(100);
                }

                // 实际项目中应实现文件上传逻辑
                // await _wallpaperService.UploadWallpaperAsync(wallpaper, FilePath);

                // 保存壁纸信息到数据库
                await _wallpaperService.AddWallpaperAsync(wallpaper);

                // 重置表单
                Title = string.Empty;
                Description = string.Empty;
                SelectedCategory = AvailableCategories.FirstOrDefault();
                SelectedResolution = AvailableResolutions.FirstOrDefault();
                FilePath = string.Empty;
                FileName = string.Empty;
                FileSize = 0;
                FileSizeText = string.Empty;
                ThumbnailData = null;
                UploadProgress = 0;

                StatusMessage = "壁纸上传成功，等待审核";
                await _dialogCoordinator.ShowMessageAsync(this, "上传成功",
                    "您的壁纸已成功上传，将在审核通过后显示");
            }
            catch (Exception ex)
            {
                ShowError($"上传失败: {ex.Message}");
            }
            finally
            {
                IsUploading = false;
            }
        }

        #endregion
    }
}