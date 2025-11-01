using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Weather.Dashboard.Avalonia.Converters
{
    /// <summary>
    /// ✅ Avalonia Converter: Temperature (double) → String with degree symbol
    /// </summary>
    public class TemperatureToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double temp)
            {
                return $"{temp:F0}°C";
            }
            return "N/A";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}