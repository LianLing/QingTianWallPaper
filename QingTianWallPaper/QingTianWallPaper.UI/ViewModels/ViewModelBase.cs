// QingTianWallPaper.UI/ViewModels/ViewModelBase.cs
using System.Reactive;
using ReactiveUI;

namespace QingTianWallPaper.UI.ViewModels
{
    public abstract class ViewModelBase : ReactiveObject
    {
        // 用于显示在窗口标题栏的标题
        private string _title;
        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        // 用于显示在状态栏的消息
        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        // 指示视图模型是否处于加载状态
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        // 指示视图模型是否有错误
        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set => this.RaiseAndSetIfChanged(ref _hasError, value);
        }

        // 错误消息
        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        // 导航命令 - 用于在视图模型之间导航
        public ReactiveCommand<string, Unit> NavigateCommand { get; }

        protected ViewModelBase()
        {
            // 默认实现 - 可以在派生类中重写
            NavigateCommand = ReactiveCommand.Create<string>(Navigate);
        }

        // 导航方法 - 可以在派生类中重写
        protected virtual void Navigate(string viewName)
        {
            // 基类中不实现具体导航逻辑
            // 派生类可以重写此方法来实现特定的导航行为
        }

        // 显示错误消息
        public void ShowError(string message)
        {
            HasError = true;
            ErrorMessage = message;
        }

        // 清除错误状态
        public void ClearError()
        {
            HasError = false;
            ErrorMessage = string.Empty;
        }
    }
}