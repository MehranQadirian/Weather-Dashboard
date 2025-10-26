using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Weather.Dashboard.Avalonia.Services
{
    /// <summary>
    /// Application configuration model
    /// </summary>
    public class AppConfiguration
    {
        public bool IsDarkTheme { get; set; }
        public string Language { get; set; } = "en";
        public bool EnableAnimations { get; set; } = true;
        public int CacheDurationMinutes { get; set; } = 10;
        public List<string> FavoriteCityIds { get; set; } = new List<string>();
        public double AnimationIntensity { get; set; } = 0.7;
        public string Version { get; set; } = "2.0.0";
        public DateTime LastModified { get; set; } = DateTime.Now;

        // Window state
        public double WindowWidth { get; set; } = 1300;
        public double WindowHeight { get; set; } = 750;
        public bool WindowMaximized { get; set; } = false;
    }

    /// <summary>
    /// Configuration service interface
    /// </summary>
    public interface IConfigurationService
    {
        AppConfiguration Configuration { get; }
        Task LoadAsync();
        Task SaveAsync();
        Task<T> GetValueAsync<T>(string key, T defaultValue = default);
        Task SetValueAsync<T>(string key, T value);
    }

    /// <summary>
    /// JSON-based configuration service
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly string _configPath;
        private AppConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        public AppConfiguration Configuration => _configuration ?? new AppConfiguration();

        public ConfigurationService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appData, "WeatherDashboard");

            try
            {
                Directory.CreateDirectory(appFolder);
                _configPath = Path.Combine(appFolder, "config.json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Failed to create config directory: {ex.Message}");
                _configPath = Path.Combine(Path.GetTempPath(), "weather_config.json");
            }

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never
            };

            _configuration = new AppConfiguration();
        }

        /// <summary>
        /// Load configuration from disk
        /// </summary>
        public async Task LoadAsync()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    _configuration = JsonSerializer.Deserialize<AppConfiguration>(json, _jsonOptions);

                    System.Diagnostics.Debug.WriteLine($"✅ Configuration loaded: Theme={(_configuration.IsDarkTheme ? "Dark" : "Light")}, Cities={_configuration.FavoriteCityIds.Count}");
                }
                else
                {
                    _configuration = new AppConfiguration
                    {
                        IsDarkTheme = DetectSystemTheme()
                    };

                    await SaveAsync();
                    System.Diagnostics.Debug.WriteLine("🆕 Created new configuration with system theme");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to load configuration: {ex.Message}");
                _configuration = new AppConfiguration();
            }
        }

        /// <summary>
        /// Save configuration to disk
        /// </summary>
        public async Task SaveAsync()
        {
            try
            {
                _configuration.LastModified = DateTime.Now;
                var json = JsonSerializer.Serialize(_configuration, _jsonOptions);
                File.WriteAllText(_configPath, json);

                System.Diagnostics.Debug.WriteLine("💾 Configuration saved successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to save configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a specific configuration value
        /// </summary>
        public Task<T> GetValueAsync<T>(string key, T defaultValue = default)
        {
            try
            {
                var property = typeof(AppConfiguration).GetProperty(key);
                if (property != null && property.CanRead)
                {
                    var value = property.GetValue(_configuration);
                    if (value is T typedValue)
                    {
                        return Task.FromResult(typedValue);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Failed to get config value '{key}': {ex.Message}");
            }

            return Task.FromResult(defaultValue);
        }

        /// <summary>
        /// Set a specific configuration value
        /// </summary>
        public async Task SetValueAsync<T>(string key, T value)
        {
            try
            {
                var property = typeof(AppConfiguration).GetProperty(key);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(_configuration, value);
                    await SaveAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Failed to set config value '{key}': {ex.Message}");
            }
        }

        /// <summary>
        /// Detect system theme preference (Windows 10/11)
        /// </summary>
        private bool DetectSystemTheme()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key?.GetValue("AppsUseLightTheme") is int themeValue)
                    {
                        return themeValue == 0; // 0 = Dark, 1 = Light
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ System theme detection failed: {ex.Message}");
            }

            return false; // Default to light theme
        }
    }
}