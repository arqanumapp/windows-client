using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Arqanum.Utilities
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool flag)
            {
                if (Invert)
                    flag = !flag;
                return flag ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility visibility)
            {
                var result = visibility == Visibility.Visible;
                return Invert ? !result : result;
            }

            return false;
        }
    }
}
