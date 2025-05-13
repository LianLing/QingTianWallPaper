using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace QingTianWallPaper.UI.Controls
{
    public class RatingToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double rating = System.Convert.ToDouble(value);
            double fractionalPart = rating - Math.Floor(rating);
            return fractionalPart;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class RatingControl : UserControl
    {
        public RatingControl()
        {
            InitializeComponent();
            DataContext = this;

            // 初始化星级集合
            Stars = new ObservableCollection<StarItem>();
            for (int i = 1; i <= MaxRating; i++)
            {
                Stars.Add(new StarItem { Value = i });
            }

            // 注册属性变更回调
            DependencyPropertyDescriptor.FromProperty(RatingProperty, typeof(RatingControl))
                .AddValueChanged(this, OnRatingChanged);
        }

        #region 依赖属性

        // 评分值
        public static readonly DependencyProperty RatingProperty =
            DependencyProperty.Register(
                "Rating",
                typeof(double),
                typeof(RatingControl),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRatingPropertyChanged));

        // 最大星数
        public static readonly DependencyProperty MaxRatingProperty =
            DependencyProperty.Register(
                "MaxRating",
                typeof(int),
                typeof(RatingControl),
                new PropertyMetadata(5, OnMaxRatingChanged));

        // 是否只读
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
                "IsReadOnly",
                typeof(bool),
                typeof(RatingControl),
                new PropertyMetadata(false));

        #endregion

        #region 属性访问器

        public double Rating
        {
            get { return (double)GetValue(RatingProperty); }
            set { SetValue(RatingProperty, value); }
        }

        public int MaxRating
        {
            get { return (int)GetValue(MaxRatingProperty); }
            set { SetValue(MaxRatingProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        #endregion

        #region 集合与命令

        public ObservableCollection<StarItem> Stars { get; private set; }

        public ICommand RateCommand => new RelayCommand<double>(value =>
        {
            if (!IsReadOnly)
            {
                Rating = value;
            }
        });

        #endregion

        #region 属性变更处理

        private static void OnRatingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as RatingControl;
            control?.UpdateStarVisuals();
        }

        private static void OnMaxRatingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as RatingControl;
            control?.UpdateStarCollection();
        }

        private void OnRatingChanged(object sender, EventArgs e)
        {
            UpdateStarVisuals();
        }

        private void UpdateStarCollection()
        {
            Stars.Clear();
            for (int i = 1; i <= MaxRating; i++)
            {
                Stars.Add(new StarItem { Value = i });
            }
            UpdateStarVisuals();
        }

        private void UpdateStarVisuals()
        {
            // 可以在这里添加动画效果
        }

        #endregion
    }

    public class StarItem
    {
        public int Value { get; set; }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }
}