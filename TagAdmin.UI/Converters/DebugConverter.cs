using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TagAdmin.UI.Converters.Base;

namespace TagAdmin.UI.Converters
{
    public class DebuggingConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value; // Add the breakpoint here!!
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
