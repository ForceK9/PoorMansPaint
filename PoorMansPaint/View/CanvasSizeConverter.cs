using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Shapes;

namespace PoorMansPaint
{
    internal class CanvasSizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double zoom = (double)values[0];
            double width = Math.Round(((double)values[1] - 2) / zoom);
            double height = Math.Round(((double)values[2] - 2) / zoom);
            return $"{width} x {height} dp";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
