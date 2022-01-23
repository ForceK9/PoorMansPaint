using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PoorMansPaint
{
    internal class PenColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush brush = (SolidColorBrush)value;
            return brush.Color.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color color = (Color)value;
            return new SolidColorBrush(color);
        }
    }
}
