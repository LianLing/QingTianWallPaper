// QingTianWallPaper.UI/Converters/PointSignConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;

namespace QingTianWallPaper.UI.Converters
{
    public class PointSignConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int points)
            {
                return points >= 0 ? "Positive" : "Negative";
            }
            return "Positive";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}