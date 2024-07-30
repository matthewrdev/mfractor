using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MFractor.VS.Windows.UI.ValueConverters
{
    public class TrueToVisibileConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => 
            value is bool boolean && boolean
                ? Visibility.Visible
                : Visibility.Hidden;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => 
            parameter is Visibility visibility && visibility == Visibility.Visible;
    }
}
