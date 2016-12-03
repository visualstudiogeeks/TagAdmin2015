using System;
using System.Globalization;
using System.Windows.Data;
using TagAdmin.Common.Entities;
using TagAdmin.Common.Extensions;
using TagAdmin.UI.Converters.Base;

namespace TagAdmin.UI.Converters
{
    public class SingleTagToEnabledConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var input = value as AsyncObservableCollection<Tag>;
            return input?.Count == 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
