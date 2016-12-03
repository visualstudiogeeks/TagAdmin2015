using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TagAdmin.UI.Converters.Base;

namespace TagAdmin.UI.Converters
{
    public class BoolToVisibilityConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = value as bool?;
            if (b.HasValue)
            {
                return b.Value ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
