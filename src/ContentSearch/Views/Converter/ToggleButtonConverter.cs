namespace ContentSearch.Views.Converter
{
    using System;
    using System.Globalization;
    using Avalonia.Data.Converters;

    public class ToggleButtonConverter : IValueConverter
    {
        public required string TrueValue { get; set; }
        public required string FalseValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked)
            {
                return this.TrueValue;
            }
            return this.FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
