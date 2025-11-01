// File: Services/SettingsService.cs
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Weather.Dashboard.Avalonia.Models;

namespace Weather.Dashboard.Avalonia.Services
{
    public interface ISettingsService
    {
        Task<SettingsModel> LoadAsync();
        Task SaveAsync(SettingsModel settings);
        Task<SettingsModel> ResetToDefaultAsync();
    }

    public class SettingsService : ISettingsService
    {
        private readonly string _settingsPath;
        private readonly JsonSerializerOptions _jsonOptions;

        public SettingsService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appData, "WeatherDashboard");
            
            try
            {
                Directory.CreateDirectory(appFolder);
                _settingsPath = Path.Combine(appFolder, "settings.json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Failed to create settings directory: {ex.Message}");
                _settingsPath = Path.Combine(Path.GetTempPath(), "weather_settings.json");
            }

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<SettingsModel> LoadAsync()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = await File.ReadAllTextAsync(_settingsPath);
                    var settings = JsonSerializer.Deserialize<SettingsModel>(json, _jsonOptions);
                    
                    if (settings != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Settings loaded: RefreshRate={settings.RefreshRateMinutes}min");
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }

            var defaultSettings = new SettingsModel();
            await SaveAsync(defaultSettings);
            System.Diagnostics.Debug.WriteLine("Created default settings");
            return defaultSettings;
        }

        public async Task SaveAsync(SettingsModel settings)
        {
            try
            {
                settings.LastModified = DateTime.Now;
                var json = JsonSerializer.Serialize(settings, _jsonOptions);
                await File.WriteAllTextAsync(_settingsPath, json);
                System.Diagnostics.Debug.WriteLine("Settings saved successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
                throw;
            }
        }

        public async Task<SettingsModel> ResetToDefaultAsync()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    File.Delete(_settingsPath);
                }

                var defaultSettings = new SettingsModel
                {
                    RefreshRateMinutes = 10,
                    DetailView = new DetailViewSettings
                    {
                        ShowCurrentWeatherCard = true,
                        ShowForecastCard = true,
                        ShowSunTimesCard = true,
                        ShowHourlyChart = true
                    },
                    CityCard = new CityCardSettings
                    {
                        ShowTemperature = true,
                        ShowHumidity = true,
                        ShowWindSpeed = true,
                        ShowPressure = true
                    },
                    Filters = new FilterSettings
                    {
                        EnableWeatherFilters = true
                    },
                    Theme = new ThemeSettings
                    {
                        EnableDynamicTheme = true,
                        FixedThemeIndex = -1
                    }
                };

                await SaveAsync(defaultSettings);
                System.Diagnostics.Debug.WriteLine("🔄 Settings reset to default");
                return defaultSettings;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to reset settings: {ex.Message}");
                throw;
            }
        }
    }
}