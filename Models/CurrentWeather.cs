using System;

namespace Weather.Dashboard.Avalonia.Models
{
    public class CurrentWeather
    {
        public double Temperature { get; set; }
        public WeatherCondition Condition { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
        public string IconCode { get; set; }
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
        public double FeelsLike { get; set; }
        public int Pressure { get; set; }
        public DateTime? Sunrise { get; set; }
        public DateTime? Sunset { get; set; }
        public double Visibility { get; set; } = 10.0; // km
        public int UvIndex { get; set; } = 5;
        public CurrentWeather()
        {
            Timestamp = DateTime.Now;
        }
    }
}