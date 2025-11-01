using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Weather.Dashboard.Avalonia.Models;
using Weather.Dashboard.Avalonia.Services;

namespace Weather.Dashboard.Avalonia.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ISettingsService _settingsService;
        private readonly MainViewModel _mainViewModel;
        private SettingsModel _originalSettings;

        private int _refreshRateMinutes;
        private bool _showCurrentWeatherCard;
        private bool _showForecastCard;
        private bool _showSunTimesCard;
        private bool _showHourlyChart;
        private bool _showTemperature;
        private bool _showHumidity;
        private bool _showWindSpeed;
        private bool _showPressure;
        private bool _enableDynamicTheme;
        private int _selectedThemeIndex;
        private bool _enableWeatherFilters;

        public event Action SettingsClosed;

        public int RefreshRateMinutes
        {
            get => _refreshRateMinutes;
            set => SetProperty(ref _refreshRateMinutes, Math.Max(1, Math.Min(120, value)));
        }

        public bool ShowCurrentWeatherCard
        {
            get => _showCurrentWeatherCard;
            set => SetProperty(ref _showCurrentWeatherCard, value);
        }

        public bool ShowForecastCard
        {
            get => _showForecastCard;
            set => SetProperty(ref _showForecastCard, value);
        }

        public bool ShowSunTimesCard
        {
            get => _showSunTimesCard;
            set => SetProperty(ref _showSunTimesCard, value);
        }

        public bool ShowHourlyChart
        {
            get => _showHourlyChart;
            set => SetProperty(ref _showHourlyChart, value);
        }

        public bool ShowTemperature
        {
            get => _showTemperature;
            set => SetProperty(ref _showTemperature, value);
        }

        public bool ShowHumidity
        {
            get => _showHumidity;
            set => SetProperty(ref _showHumidity, value);
        }

        public bool ShowWindSpeed
        {
            get => _showWindSpeed;
            set => SetProperty(ref _showWindSpeed, value);
        }

        public bool ShowPressure
        {
            get => _showPressure;
            set => SetProperty(ref _showPressure, value);
        }

        public bool EnableDynamicTheme
        {
            get => _enableDynamicTheme;
            set => SetProperty(ref _enableDynamicTheme, value);
        }

        public int SelectedThemeIndex
        {
            get => _selectedThemeIndex;
            set => SetProperty(ref _selectedThemeIndex, value);
        }

        public bool EnableWeatherFilters
        {
            get => _enableWeatherFilters;
            set => SetProperty(ref _enableWeatherFilters, value);
        }

        public RelayCommand ApplyCommand { get; }
        public RelayCommand ResetCommand { get; }
        public RelayCommand CloseCommand { get; }
        public RelayCommand<int> SelectThemeCommand { get; }

        public SettingsViewModel(ISettingsService settingsService, MainViewModel mainViewModel)
        {
            _settingsService = settingsService;
            _mainViewModel = mainViewModel;

            ApplyCommand = new RelayCommand(async _ => await ApplySettingsAsync());
            ResetCommand = new RelayCommand(async _ => await ResetToDefaultsAsync());
            CloseCommand = new RelayCommand(_ => Close());
            SelectThemeCommand = new RelayCommand<int>(index => SelectTheme(index));

            _ = LoadSettingsAsync();
        }

        private async Task LoadSettingsAsync()
        {
            try
            {
                _originalSettings = await _settingsService.LoadAsync();

                RefreshRateMinutes = _originalSettings.RefreshRateMinutes;
                ShowCurrentWeatherCard = _originalSettings.DetailView.ShowCurrentWeatherCard;
                ShowForecastCard = _originalSettings.DetailView.ShowForecastCard;
                ShowSunTimesCard = _originalSettings.DetailView.ShowSunTimesCard;
                ShowHourlyChart = _originalSettings.DetailView.ShowHourlyChart;
                ShowTemperature = _originalSettings.CityCard.ShowTemperature;
                ShowHumidity = _originalSettings.CityCard.ShowHumidity;
                ShowWindSpeed = _originalSettings.CityCard.ShowWindSpeed;
                ShowPressure = _originalSettings.CityCard.ShowPressure;

                EnableWeatherFilters = _originalSettings.Filters.EnableWeatherFilters;

                EnableDynamicTheme = _originalSettings.Theme.EnableDynamicTheme;
                SelectedThemeIndex = _originalSettings.Theme.FixedThemeIndex;

                System.Diagnostics.Debug.WriteLine("✅ Settings loaded into ViewModel:");
                System.Diagnostics.Debug.WriteLine($"   EnableWeatherFilters: {EnableWeatherFilters}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to load settings: {ex.Message}");
            }
        }

        private async Task ApplySettingsAsync()
        {
            try
            {
                var changes = GetChanges();
                if (!changes.Any())
                {
                    System.Diagnostics.Debug.WriteLine("ℹ️ No changes detected, closing settings");
                    Close();
                    return;
                }

                var newSettings = new SettingsModel
                {
                    RefreshRateMinutes = RefreshRateMinutes,
                    DetailView = new DetailViewSettings
                    {
                        ShowCurrentWeatherCard = ShowCurrentWeatherCard,
                        ShowForecastCard = ShowForecastCard,
                        ShowSunTimesCard = ShowSunTimesCard,
                        ShowHourlyChart = ShowHourlyChart
                    },
                    CityCard = new CityCardSettings
                    {
                        ShowTemperature = ShowTemperature,
                        ShowHumidity = ShowHumidity,
                        ShowWindSpeed = ShowWindSpeed,
                        ShowPressure = ShowPressure
                    },
                    Filters = new FilterSettings
                    {
                        EnableWeatherFilters = EnableWeatherFilters
                    },
                    Theme = new ThemeSettings
                    {
                        EnableDynamicTheme = EnableDynamicTheme,
                        FixedThemeIndex = SelectedThemeIndex
                    }
                };
                await _settingsService.SaveAsync(newSettings);
                System.Diagnostics.Debug.WriteLine($"💾 Settings saved to disk - {changes.Count} changes:");

                foreach (var change in changes)
                {
                    System.Diagnostics.Debug.WriteLine($"   • {change}");
                }

                ApplyThemeChangesImmediately(newSettings.Theme);

                ApplyToMainViewModel(newSettings);

                await Task.Delay(100);

                _originalSettings = newSettings;
                System.Diagnostics.Debug.WriteLine("✅ All settings applied successfully");

                Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to apply settings: {ex.Message}");
            }
        }

        private void ApplyToMainViewModel(SettingsModel settings)
        {
            System.Diagnostics.Debug.WriteLine("🔄 Applying settings to MainViewModel...");

            _mainViewModel.ShowCurrentWeatherCard = settings.DetailView.ShowCurrentWeatherCard;
            _mainViewModel.ShowForecastCard = settings.DetailView.ShowForecastCard;
            _mainViewModel.ShowSunTimesCard = settings.DetailView.ShowSunTimesCard;
            _mainViewModel.ShowHourlyChart = settings.DetailView.ShowHourlyChart;

            _mainViewModel.ShowTemperature = settings.CityCard.ShowTemperature;
            _mainViewModel.ShowHumidity = settings.CityCard.ShowHumidity;
            _mainViewModel.ShowWindSpeed = settings.CityCard.ShowWindSpeed;
            _mainViewModel.ShowPressure = settings.CityCard.ShowPressure;

            _mainViewModel.RefreshRateMinutes = settings.RefreshRateMinutes;
            _mainViewModel.EnableWeatherFilters = settings.Filters.EnableWeatherFilters;

            System.Diagnostics.Debug.WriteLine($"📊 MainViewModel Updated:");
            System.Diagnostics.Debug.WriteLine($"   EnableWeatherFilters: {_mainViewModel.EnableWeatherFilters}");
            System.Diagnostics.Debug.WriteLine($"   IsFilterBarVisible: {_mainViewModel.IsFilterBarVisible}");

            System.Diagnostics.Debug.WriteLine("✅ Settings applied to MainViewModel");
        }

        private void ApplyThemeChangesImmediately(ThemeSettings themeSettings)
        {
            try
            {
                var circadianService = ((App)Application.Current).ServiceProvider?.GetService<ICircadianThemeService>();

                if (circadianService == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ CircadianThemeService not available");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"🎨 Theme Settings:");
                System.Diagnostics.Debug.WriteLine($"   EnableDynamicTheme: {themeSettings.EnableDynamicTheme}");
                System.Diagnostics.Debug.WriteLine($"   FixedThemeIndex: {themeSettings.FixedThemeIndex}");

                if (themeSettings.EnableDynamicTheme)
                {
                    System.Diagnostics.Debug.WriteLine("🌓 Applying dynamic (circadian) theme");
                    circadianService.StopDynamicTheming();
                    circadianService.StartDynamicTheming();
                    System.Diagnostics.Debug.WriteLine("✅ Dynamic theme started");
                }
                else if (themeSettings.FixedThemeIndex >= 0)
                {
                    var period = (CircadianPeriod)themeSettings.FixedThemeIndex;
                    System.Diagnostics.Debug.WriteLine($"🎨 Applying fixed theme: {period}");
                    circadianService.StopDynamicTheming();
                    circadianService.ApplyThemeForPeriod(period);
                    System.Diagnostics.Debug.WriteLine($"✅ Fixed theme applied: {period}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("🌓 No valid theme, applying dynamic");
                    circadianService.StopDynamicTheming();
                    circadianService.StartDynamicTheming();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to apply theme: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   Stack: {ex.StackTrace}");
            }
        }

        private List<string> GetChanges()
        {
            var changes = new List<string>();
            if (_originalSettings == null) return changes;

            if (RefreshRateMinutes != _originalSettings.RefreshRateMinutes)
                changes.Add($"Refresh Rate: {_originalSettings.RefreshRateMinutes}min → {RefreshRateMinutes}min");

            if (ShowCurrentWeatherCard != _originalSettings.DetailView.ShowCurrentWeatherCard)
                changes.Add(
                    $"Current Weather Card: {GetToggleText(_originalSettings.DetailView.ShowCurrentWeatherCard, ShowCurrentWeatherCard)}");

            if (ShowForecastCard != _originalSettings.DetailView.ShowForecastCard)
                changes.Add(
                    $"Forecast Card: {GetToggleText(_originalSettings.DetailView.ShowForecastCard, ShowForecastCard)}");

            if (ShowSunTimesCard != _originalSettings.DetailView.ShowSunTimesCard)
                changes.Add(
                    $"Sun Times Card: {GetToggleText(_originalSettings.DetailView.ShowSunTimesCard, ShowSunTimesCard)}");

            if (ShowHourlyChart != _originalSettings.DetailView.ShowHourlyChart)
                changes.Add(
                    $"Hourly Chart: {GetToggleText(_originalSettings.DetailView.ShowHourlyChart, ShowHourlyChart)}");

            if (ShowTemperature != _originalSettings.CityCard.ShowTemperature)
                changes.Add(
                    $"Temperature Field: {GetToggleText(_originalSettings.CityCard.ShowTemperature, ShowTemperature)}");

            if (ShowHumidity != _originalSettings.CityCard.ShowHumidity)
                changes.Add($"Humidity Field: {GetToggleText(_originalSettings.CityCard.ShowHumidity, ShowHumidity)}");

            if (ShowWindSpeed != _originalSettings.CityCard.ShowWindSpeed)
                changes.Add(
                    $"Wind Speed Field: {GetToggleText(_originalSettings.CityCard.ShowWindSpeed, ShowWindSpeed)}");

            if (ShowPressure != _originalSettings.CityCard.ShowPressure)
                changes.Add($"Pressure Field: {GetToggleText(_originalSettings.CityCard.ShowPressure, ShowPressure)}");

            if (EnableWeatherFilters != _originalSettings.Filters.EnableWeatherFilters)
                changes.Add(
                    $"Weather Filters: {GetToggleText(_originalSettings.Filters.EnableWeatherFilters, EnableWeatherFilters)}");

            if (EnableDynamicTheme != _originalSettings.Theme.EnableDynamicTheme)
                changes.Add(
                    $"Dynamic Theme: {GetToggleText(_originalSettings.Theme.EnableDynamicTheme, EnableDynamicTheme)}");

            if (!EnableDynamicTheme && SelectedThemeIndex != _originalSettings.Theme.FixedThemeIndex)
                changes.Add($"Fixed Theme: Changed to {GetThemeName(SelectedThemeIndex)}");

            return changes;
        }

        private string GetToggleText(bool old, bool newVal)
        {
            return $"{(old ? "Enabled" : "Disabled")} → {(newVal ? "Enabled" : "Disabled")}";
        }

        private string GetThemeName(int index)
        {
            return ((CircadianPeriod)index) switch
            {
                CircadianPeriod.DeepNight => "Deep Night",
                CircadianPeriod.LateNight => "Late Night",
                CircadianPeriod.PreDawn => "Pre-Dawn",
                CircadianPeriod.Dawn => "Dawn",
                CircadianPeriod.EarlyMorning => "Early Morning",
                CircadianPeriod.LateMorning => "Late Morning",
                CircadianPeriod.Noon => "Noon",
                CircadianPeriod.Afternoon => "Afternoon",
                CircadianPeriod.LateAfternoon => "Late Afternoon",
                CircadianPeriod.Dusk => "Dusk",
                CircadianPeriod.Evening => "Evening",
                CircadianPeriod.Night => "Night",
                _ => "Unknown"
            };
        }

        private async Task ResetToDefaultsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Resetting to defaults...");

                var defaultSettings = await _settingsService.ResetToDefaultAsync();

                RefreshRateMinutes = defaultSettings.RefreshRateMinutes;
                ShowCurrentWeatherCard = defaultSettings.DetailView.ShowCurrentWeatherCard;
                ShowForecastCard = defaultSettings.DetailView.ShowForecastCard;
                ShowSunTimesCard = defaultSettings.DetailView.ShowSunTimesCard;
                ShowHourlyChart = defaultSettings.DetailView.ShowHourlyChart;

                ShowTemperature = defaultSettings.CityCard.ShowTemperature;
                ShowHumidity = defaultSettings.CityCard.ShowHumidity;
                ShowWindSpeed = defaultSettings.CityCard.ShowWindSpeed;
                ShowPressure = defaultSettings.CityCard.ShowPressure;

                EnableWeatherFilters = defaultSettings.Filters.EnableWeatherFilters;
                EnableDynamicTheme = defaultSettings.Theme.EnableDynamicTheme;
                SelectedThemeIndex = defaultSettings.Theme.FixedThemeIndex;

                _originalSettings = defaultSettings;

                System.Diagnostics.Debug.WriteLine("📄 ViewModel reset to defaults:");
                System.Diagnostics.Debug.WriteLine($"   EnableDynamicTheme: {EnableDynamicTheme}");
                System.Diagnostics.Debug.WriteLine($"   FixedThemeIndex: {SelectedThemeIndex}");
                ApplyThemeChangesImmediately(defaultSettings.Theme);
                ApplyToMainViewModel(defaultSettings);

                System.Diagnostics.Debug.WriteLine("✅ Settings reset completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to reset settings: {ex.Message}");
            }
        }

        private void SelectTheme(int themeIndex)
        {
            System.Diagnostics.Debug.WriteLine($"🎨 Theme selected manually: {(CircadianPeriod)themeIndex}");
            EnableDynamicTheme = false;
            SelectedThemeIndex = themeIndex;
            System.Diagnostics.Debug.WriteLine($"   EnableDynamicTheme: {EnableDynamicTheme}");
            System.Diagnostics.Debug.WriteLine($"   SelectedThemeIndex: {SelectedThemeIndex}");
        }

        private void Close()
        {
            System.Diagnostics.Debug.WriteLine("🚪 SettingsViewModel closing");
            SettingsClosed?.Invoke();
        }
    }
}