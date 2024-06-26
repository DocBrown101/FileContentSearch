﻿namespace ContentSearch.Views.Converter
{
    using System;
    using System.Globalization;
    using Avalonia.Data.Converters;

    // Convert an integer percentage (0 to 100) to a double between (0.0d and 1.0d)
    public class IntPercentageToDoubleConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var res = 0.0d;
            if (value is int intVal)
            {
                res = intVal / 100.0d;

                if (res < 0.0d)
                {
                    res = 0.0d;
                }
                else if (res > 100.0d)
                {
                    res = 100.0d;
                }
            }

            return res;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
