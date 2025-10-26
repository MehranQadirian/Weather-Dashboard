using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Weather.Dashboard.Avalonia.Models;

namespace Weather.Dashboard.Avalonia.Converters
{
    /// <summary>
    /// ✅ Avalonia Converter: WeatherCondition → Color
    /// </summary>
    public class WeatherConditionToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is WeatherCondition condition)
            {
                return condition switch
                {
                    WeatherCondition.Sunny => Color.FromRgb(255, 213, 79),
                    WeatherCondition.Cloudy => Color.FromRgb(144, 164, 174),
                    WeatherCondition.Rainy => Color.FromRgb(66, 165, 245),
                    WeatherCondition.Snowy => Color.FromRgb(227, 242, 253),
                    WeatherCondition.Storm => Color.FromRgb(94, 53, 177),
                    WeatherCondition.PartlyCloudy => Color.FromRgb(255, 224, 130),
                    WeatherCondition.Foggy => Color.FromRgb(161, 161, 170),
                    _ => Colors.Gray
                };
            }
            return Colors.Gray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}