using System;

namespace Weather.Dashboard.Avalonia.Models
{
    public class ForecastItem
    {
        public DateTime DateTime { get; set; }
        public double MinTemp { get; set; }
        public double MaxTemp { get; set; }
        public WeatherCondition Condition { get; set; }
        public int PrecipitationProb { get; set; }
        public string IconCode { get; set; }
        public double Humidity { get; set; }
        public double WindSpeed { get; set; }
    }
}