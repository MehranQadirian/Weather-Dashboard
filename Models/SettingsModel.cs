using System;

namespace Weather.Dashboard.Avalonia.Models
{
    public class SettingsModel
    {
        public int RefreshRateMinutes { get; set; } = 10;
        public DetailViewSettings DetailView { get; set; } = new();
        public CityCardSettings CityCard { get; set; } = new();
        public FilterSettings Filters { get; set; } = new();
        public ThemeSettings Theme { get; set; } = new();
        public DateTime LastModified { get; set; } = DateTime.Now;
    }

    public class DetailViewSettings
    {
        public bool ShowCurrentWeatherCard { get; set; } = true;
        public bool ShowForecastCard { get; set; } = true;
        public bool ShowSunTimesCard { get; set; } = true;
        public bool ShowHourlyChart { get; set; } = true;
    }

    public class CityCardSettings
    {
        public bool ShowTemperature { get; set; } = true;
        public bool ShowHumidity { get; set; } = true;
        public bool ShowWindSpeed { get; set; } = true;
        public bool ShowPressure { get; set; } = true;
    }

    public class FilterSettings
    {
        public bool EnableWeatherFilters { get; set; } = true;
    }

    public class ThemeSettings
    {
        public bool EnableDynamicTheme { get; set; } = true;
        public int FixedThemeIndex { get; set; } = -1;
    }
}