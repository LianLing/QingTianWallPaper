using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QingTianWallPaper.Core.Entities;
using QingTianWallPaper.Core.Models;

namespace QingTianWallPaper.UI.Controls
{
    public partial class WallpaperCard : UserControl
    {
        #region 依赖属性

        // 点击卡片时的命令
        public static readonly DependencyProperty CardClickCommandProperty =
            DependencyProperty.Register(
                "CardClickCommand",
                typeof(ICommand),
                typeof(WallpaperCard),
                new PropertyMetadata(null));

        // 壁纸数据上下文
        public static readonly DependencyProperty WallpaperProperty =
            DependencyProperty.Register(
                "Wallpaper",
                typeof(Wallpaper),
                typeof(WallpaperCard),
                new PropertyMetadata(null, OnWallpaperChanged));

        #endregion

        #region 属性访问器

        public ICommand CardClickCommand
        {
            get { return (ICommand)GetValue(CardClickCommandProperty); }
            set { SetValue(CardClickCommandProperty, value); }
        }

        public Wallpaper Wallpaper
        {
            get { return (Wallpaper)GetValue(WallpaperProperty); }
            set { SetValue(WallpaperProperty, value); }
        }

        #endregion

        public WallpaperCard()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // 当DataContext变更时，同步Wallpaper属性
            if (DataContext is Wallpaper wallpaper)
            {
                Wallpaper = wallpaper;
            }
        }

        private static void OnWallpaperChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 壁纸数据变更时的处理逻辑
            var card = d as WallpaperCard;
            // 可以在这里添加数据变更后的更新逻辑
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 卡片点击事件处理
            if (CardClickCommand != null && CardClickCommand.CanExecute(Wallpaper))
            {
                CardClickCommand.Execute(Wallpaper);
            }
            e.Handled = true;
        }
    }
}