using System;

namespace Weather.Dashboard.Avalonia.Models
{
    public class AnimationState
    {
        public WeatherCondition Condition { get; set; }
        public double Intensity { get; set; }
        public bool IsNight { get; set; }
        public int ParticleCount { get; set; }
        public double WindAngle { get; set; }
        public double CloudSpeed { get; set; }

        public AnimationState()
        {
            Intensity = 0.5;
            ParticleCount = 80;
            WindAngle = 10;
            CloudSpeed = 1.0;
        }

        public static AnimationState FromWeather(CurrentWeather weather)
        {
            if (weather == null)
            {
                return new AnimationState
                {
                    Condition = WeatherCondition.PartlyCloudy,
                    IsNight = DetermineNightTimeByLocation(null),
                    Intensity = 0.3
                };
            }

            var state = new AnimationState
            {
                Condition = weather.Condition,
                IsNight = DetermineNightTimeByLocation(weather)
            };

            switch (weather.Condition)
            {
                case WeatherCondition.Rainy:
                    state.Intensity = Math.Min(1.0, weather.Humidity / 80.0);
                    state.ParticleCount = (int)(120 * state.Intensity);
                    state.WindAngle = CalculateWindAngle(weather.WindSpeed);
                    break;

                case WeatherCondition.Storm:
                    state.Intensity = 0.9 + (weather.WindSpeed / 100.0);
                    state.Intensity = Math.Min(1.0, state.Intensity);
                    state.ParticleCount = (int)(180 * state.Intensity);
                    state.WindAngle = CalculateWindAngle(weather.WindSpeed);
                    state.CloudSpeed = 2.0 + (weather.WindSpeed / 10.0);
                    break;

                case WeatherCondition.Snowy:
                    state.Intensity = Math.Max(0.4, 1.0 - (weather.Temperature / 10.0));
                    state.Intensity = Math.Max(0.0, Math.Min(1.0, state.Intensity));
                    state.ParticleCount = (int)(70 * state.Intensity);
                    state.WindAngle = CalculateWindAngle(weather.WindSpeed) * 0.5;
                    break;

                case WeatherCondition.Cloudy:
                    state.Intensity = 0.3;
                    state.CloudSpeed = 0.5 + (weather.WindSpeed / 15.0);
                    state.ParticleCount = 0;
                    break;

                case WeatherCondition.PartlyCloudy:
                    state.Intensity = 0.4;
                    state.CloudSpeed = 0.7 + (weather.WindSpeed / 20.0);
                    state.ParticleCount = 0;
                    break;

                case WeatherCondition.Sunny:
                    state.Intensity = 0.5;
                    state.ParticleCount = 0;
                    break;

                case WeatherCondition.Foggy:
                    state.Intensity = Math.Min(1.0, weather.Humidity / 70.0);
                    state.ParticleCount = 0;
                    break;

                default:
                    state.Intensity = 0.3;
                    state.ParticleCount = 0;
                    break;
            }

            System.Diagnostics.Debug.WriteLine(
                $"🎨 Animation State: {state.Condition}, " +
                $"Intensity: {state.Intensity:P0}, " +
                $"Night: {state.IsNight}, " +
                $"Particles: {state.ParticleCount}");

            return state;
        }

        private static bool DetermineNightTimeByLocation(CurrentWeather weather)
        {
            if (weather?.Sunrise == null || weather?.Sunset == null)
                return IsNightTimeSystemBased();

            var now = DateTime.Now;
            var sunrise = weather.Sunrise.Value;
            var sunset = weather.Sunset.Value;

            return now.TimeOfDay < sunrise.TimeOfDay || now.TimeOfDay > sunset.TimeOfDay;
        }

        private static bool IsNightTimeSystemBased()
        {
            var hour = DateTime.Now.Hour;
            return hour < 6 || hour >= 18;
        }

        private static double CalculateWindAngle(double windSpeed)
        {
            return Math.Min(30, windSpeed * 2);
        }
    }
}