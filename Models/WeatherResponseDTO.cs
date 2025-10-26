using System;
using System.Collections.Generic;

namespace Weather.Dashboard.Avalonia.Models
{
    public class WeatherResponseDTO
    {
        public CurrentWeatherDTO Current { get; set; }
        public List<ForecastDTO> Forecast { get; set; }
        public DateTime CachedAt { get; set; }
        public int CacheTTLMinutes { get; set; }

        public bool IsExpired()
        {
            return DateTime.Now.Subtract(CachedAt).TotalMinutes > CacheTTLMinutes;
        }
    }

    public class CurrentWeatherDTO
    {
        public double Temp { get; set; }
        public double FeelsLike { get; set; }
        public int Humidity { get; set; }
        public int Pressure { get; set; }
        public double WindSpeed { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Main { get; set; }
    }

    public class ForecastDTO
    {
        public long Dt { get; set; }
        public TempDTO Temp { get; set; }
        public double Humidity { get; set; }
        public double WindSpeed { get; set; }
        public List<WeatherDescDTO> Weather { get; set; }
        public double Pop { get; set; } // Precipitation probability
    }

    public class TempDTO
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public double Day { get; set; }
    }

    public class WeatherDescDTO
    {
        public string Main { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
    }
}
