using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weather.Dashboard.Avalonia.Models;
using Weather.Dashboard.Avalonia.Services.Interfaces;

namespace Weather.Dashboard.Avalonia.ViewModels
{
    public class CityWeatherViewModel : BaseViewModel
    {
        private readonly IWeatherApiService _weatherService;
        private readonly IAnimationService _animationService;
        private City _city;
        private CurrentWeather _currentWeather;
        private ForecastItem[] _forecast;
        private bool _isLoading;
        private AnimationState _animationState;
        private string _errorMessage;
        private List<double> _hourlyTemperatures;

        public City City
        {
            get => _city;
            set => SetProperty(ref _city, value);
        }

        public CurrentWeather CurrentWeather
        {
            get => _currentWeather;
            set
            {
                if (SetProperty(ref _currentWeather, value))
                {
                    UpdateAnimationParameters();
                }
            }
        }

        public new void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
        }

        public ForecastItem[] Forecast
        {
            get => _forecast;
            set => SetProperty(ref _forecast, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public AnimationState AnimationState
        {
            get => _animationState;
            set => SetProperty(ref _animationState, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public List<double> HourlyTemperatures
        {
            get => _hourlyTemperatures;
            set
            {
                if (SetProperty(ref _hourlyTemperatures, value))
                {
                    System.Diagnostics.Debug.WriteLine($"📊 HourlyTemperatures updated for {City?.Name}: {value?.Count ?? 0} points");
                }
            }
        }

        public CityWeatherViewModel(City city, IWeatherApiService weatherService, IAnimationService animationService)
        {
            _city = city;
            _weatherService = weatherService;
            _animationService = animationService;
            _ = LoadWeatherAsync();
        }

        public async Task LoadWeatherAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            System.Diagnostics.Debug.WriteLine($"📊 Loading weather for {City.Name}...");
            
            CurrentWeather = await _weatherService.GetCurrentWeatherAsync(City.Lat, City.Lon);
            System.Diagnostics.Debug.WriteLine($"✅ Current weather loaded: {CurrentWeather.Temperature}°C");

            Forecast = await _weatherService.GetForecastAsync(City.Lat, City.Lon);
            
            System.Diagnostics.Debug.WriteLine($"📋 FORECAST COUNT: {Forecast?.Length ?? 0} days received");
            if (Forecast != null)
            {
                for (int i = 0; i < Forecast.Length; i++)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"   [{i}] {Forecast[i].DateTime:yyyy-MM-dd dddd} - {Forecast[i].Condition} - Max: {Forecast[i].MaxTemp}°C, Min: {Forecast[i].MinTemp}°C");
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"✅ Forecast loaded: {Forecast?.Length ?? 0} days");

            var hourlyData = await _weatherService.GetHourlyTemperaturesAsync(City.Lat, City.Lon);
            if (hourlyData != null && hourlyData.Length > 0)
            {
                HourlyTemperatures = new List<double>(hourlyData);
                System.Diagnostics.Debug.WriteLine($"✅ Hourly temperatures loaded: {HourlyTemperatures.Count} points");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ No hourly data received from API");
                HourlyTemperatures = null;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Unable to connect. Check your internet.";
            System.Diagnostics.Debug.WriteLine($"❌ LoadWeatherAsync failed: {ex.Message}");
            CurrentWeather = null;
            Forecast = null;
            HourlyTemperatures = null;
        }
        finally
        {
            IsLoading = false;
        }
    }

        private void UpdateAnimationParameters()
        {
            if (CurrentWeather != null)
            {
                AnimationState = _animationService.GetAnimationState(CurrentWeather);
                var isNightAtLocation = DetermineNightAtLocation();
                System.Diagnostics.Debug.WriteLine($"🌓 Location: {City.Name} | " +
                    $"Is Night: {isNightAtLocation} | " +
                    $"Sunrise: {CurrentWeather.Sunrise:HH:mm} | " +
                    $"Sunset: {CurrentWeather.Sunset:HH:mm} | " +
                    $"Current Time (System): {DateTime.Now:HH:mm}");
            }
        }

        private bool DetermineNightAtLocation()
        {
            if (CurrentWeather?.Sunrise == null || CurrentWeather?.Sunset == null)
                return IsNightTime();

            var now = DateTime.Now;
            var sunrise = CurrentWeather.Sunrise.Value;
            var sunset = CurrentWeather.Sunset.Value;

            bool isNight = now.TimeOfDay < sunrise.TimeOfDay || now.TimeOfDay > sunset.TimeOfDay;
            return isNight;
        }

        private bool IsNightTime()
        {
            var hour = DateTime.Now.Hour;
            return hour < 6 || hour >= 18;
        }
    }
}