namespace ContentSearch.Views.Converter
{
    using System;
    using System.Globalization;
    using Avalonia.Data.Converters;

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool inputValue ? !inputValue : true;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool inputValue ? !inputValue : true;
        }
    }
}
