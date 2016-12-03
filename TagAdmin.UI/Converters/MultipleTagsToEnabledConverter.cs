using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TagAdmin.Common.Entities;
using TagAdmin.Common.Extensions;
using TagAdmin.UI.Converters.Base;

namespace TagAdmin.UI.Converters
{
    public class MultipleTagsToEnabledConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var input = value as AsyncObservableCollection<Tag>;
            if (input?.Count > 1)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
