using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Weather.Dashboard.Avalonia.Models;
using Weather.Dashboard.Avalonia.Services;
using Weather.Dashboard.Avalonia.Services.Interfaces;

namespace Weather.Dashboard.Avalonia.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        private readonly IWeatherApiService _weatherService;
        private readonly IAnimationService _animationService;
        private readonly IStorageService _storageService;
        private readonly ISettingsService _settingsService;
        private System.Threading.CancellationTokenSource _searchCts;
        private DispatcherTimer _refreshTimer;

        private string _currentPeriod;

        private bool _showCurrentWeatherCard;
        private bool _showForecastCard;
        private bool _showSunTimesCard;
        private bool _showHourlyChart;

        private bool _showTemperature;
        private bool _showHumidity;
        private bool _showWindSpeed;
        private bool _showPressure;

        private int _refreshRateMinutes = 10;
        private bool _isFilterBarVisible;
        private int _cardsGridRow = 2;
        private int _cardsGridRowSpan = 1;
        private bool _enableWeatherFilters;

        public bool EnableWeatherFilters
        {
            get => _enableWeatherFilters;
            set
            {
                System.Diagnostics.Debug.WriteLine(
                    $"⚙️ EnableWeatherFilters setter called: {_enableWeatherFilters} → {value}");

                if (SetProperty(ref _enableWeatherFilters, value))
                {
                    System.Diagnostics.Debug.WriteLine($"✅ EnableWeatherFilters changed to: {value}");
                    UpdateUILayout();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ EnableWeatherFilters NOT changed (same value)");
                }
            }
        }

        public bool IsFilterBarVisible
        {
            get => _isFilterBarVisible;
            set
            {
                System.Diagnostics.Debug.WriteLine(
                    $"⚙️ IsFilterBarVisible setter called: {_isFilterBarVisible} → {value}");

                if (SetProperty(ref _isFilterBarVisible, value))
                {
                    System.Diagnostics.Debug.WriteLine($"✅ IsFilterBarVisible changed to: {value}");
                    UpdateCardsGridPosition();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ IsFilterBarVisible NOT changed (same value: {value})");
                }
            }
        }

        public int CardsGridRow
        {
            get => _cardsGridRow;
            set => SetProperty(ref _cardsGridRow, value);
        }

        public int CardsGridRowSpan
        {
            get => _cardsGridRowSpan;
            set => SetProperty(ref _cardsGridRowSpan, value);
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

        public int RefreshRateMinutes
        {
            get => _refreshRateMinutes;
            set
            {
                if (SetProperty(ref _refreshRateMinutes, Math.Max(1, Math.Min(120, value))))
                {
                    UpdateRefreshTimer();
                }
            }
        }

        public RelayCommand OpenSettingsCommand { get; }

        public string CurrentPeriod
        {
            get => _currentPeriod;
            set => SetProperty(ref _currentPeriod, value);
        }

        private ObservableCollection<CityWeatherViewModel> _favorites;
        private CityWeatherViewModel _selectedCity;
        private bool _isDarkTheme;
        private string _searchQuery;
        private ObservableCollection<City> _searchResults;
        private bool _isSearching;
        private DateTime _lastUpdateTime;
        private bool _hasSearchResults;
        private bool _isInitializing;
        private string _statusMessage;
        private WeatherCondition? _selectedFilter;
        private ObservableCollection<CityWeatherViewModel> _filteredFavorites;
        private string _searchHintText;

        public RelayCommand<WeatherCondition?> SetFilterCommand { get; }

        public ObservableCollection<CityWeatherViewModel> Favorites
        {
            get => _favorites;
            set => SetProperty(ref _favorites, value);
        }

        public ObservableCollection<CityWeatherViewModel> FilteredFavorites
        {
            get => _filteredFavorites;
            set => SetProperty(ref _filteredFavorites, value);
        }

        public WeatherCondition? SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (SetProperty(ref _selectedFilter, value))
                {
                    ApplyFilter();
                }
            }
        }

        public CityWeatherViewModel SelectedCity
        {
            get => _selectedCity;
            set => SetProperty(ref _selectedCity, value);
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    _ = SearchCitiesAsync();
                }
            }
        }

        public ObservableCollection<City> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }

        public bool IsSearching
        {
            get => _isSearching;
            set => SetProperty(ref _isSearching, value);
        }

        public bool HasSearchResults
        {
            get => _hasSearchResults;
            set => SetProperty(ref _hasSearchResults, value);
        }

        public DateTime LastUpdateTime
        {
            get => _lastUpdateTime;
            set => SetProperty(ref _lastUpdateTime, value);
        }

        public bool IsInitializing
        {
            get => _isInitializing;
            set => SetProperty(ref _isInitializing, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string SearchHintText
        {
            get => _searchHintText;
            set => SetProperty(ref _searchHintText, value);
        }

        public RelayCommand ClearSearchCommand { get; }
        public RelayCommand<City> AddCityCommand { get; }
        public RelayCommand<CityWeatherViewModel> RemoveCityCommand { get; }
        public RelayCommand RefreshAllCommand { get; }
        public RelayCommand ToggleThemeCommand { get; }
        public RelayCommand<CityWeatherViewModel> SelectCityCommand { get; }

        public MainViewModel(
            IWeatherApiService weatherService,
            IAnimationService animationService,
            IStorageService storageService,
            ISettingsService settingsService)
        {
            _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
            _animationService = animationService ?? throw new ArgumentNullException(nameof(animationService));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            if (Application.Current != null)
            {
                var circadianService = ((App)Application.Current).ServiceProvider?.GetService<ICircadianThemeService>();
                if (circadianService != null)
                {
                    circadianService.ThemeChanged += (s, period) => { CurrentPeriod = GetPersianPeriodName(period); };
                    CurrentPeriod = GetPersianPeriodName(circadianService.GetCurrentPeriod());
                }
            }

            _showCurrentWeatherCard = true;
            _showForecastCard = true;
            _showSunTimesCard = true;
            _showHourlyChart = true;
            _enableWeatherFilters = true;
            _isFilterBarVisible = true;
            _showTemperature = true;
            _showHumidity = true;
            _showWindSpeed = true;
            _showPressure = true;

            OpenSettingsCommand = new RelayCommand(_ => OnOpenSettings());

            Favorites = new ObservableCollection<CityWeatherViewModel>();
            FilteredFavorites = new ObservableCollection<CityWeatherViewModel>();
            SearchResults = new ObservableCollection<City>();
            SearchHintText = "Search cities... or use 'condition:city' (e.g., 'rainy:tehran')";

            SetFilterCommand = new RelayCommand<WeatherCondition?>(filter => SelectedFilter = filter);
            LastUpdateTime = DateTime.Now;

            ClearSearchCommand = new RelayCommand(_ => ClearSearch());
            AddCityCommand = new RelayCommand<City>(async city => await AddCityAsync(city));
            RemoveCityCommand = new RelayCommand<CityWeatherViewModel>(async vm => await RemoveCityAsync(vm));
            RefreshAllCommand = new RelayCommand(async _ => await RefreshAllAsync());
            SelectCityCommand = new RelayCommand<CityWeatherViewModel>(city => SelectedCity = city);

            InitializeRefreshTimer();
            _ = InitializeAsync();
        }

        private void UpdateUILayout()
        {
            System.Diagnostics.Debug.WriteLine(
                $"🔄 UpdateUILayout called - EnableWeatherFilters: {EnableWeatherFilters}");

            IsFilterBarVisible = EnableWeatherFilters;
            UpdateCardsGridPosition();

            System.Diagnostics.Debug.WriteLine($"   → IsFilterBarVisible set to: {IsFilterBarVisible}");
            System.Diagnostics.Debug.WriteLine($"   → CardsGridRow: {CardsGridRow}, RowSpan: {CardsGridRowSpan}");
        }

        private void InitializeRefreshTimer()
        {
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _refreshTimer.Tick += RefreshTimer_Tick;
        }

        private void UpdateCardsGridPosition()
        {
            if (EnableWeatherFilters)
            {
                CardsGridRow = 2;
                CardsGridRowSpan = 1;
                System.Diagnostics.Debug.WriteLine("📐 Cards Grid: Row=2, RowSpan=1 (FilterBar visible)");
            }
            else
            {
                CardsGridRow = 1;
                CardsGridRowSpan = 2;
                System.Diagnostics.Debug.WriteLine("📐 Cards Grid: Row=1, RowSpan=2 (FilterBar hidden)");
            }
        }


        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            _ = RefreshAllAsync();
        }

        private void UpdateRefreshTimer()
        {
            if (_refreshTimer != null)
            {
                _refreshTimer.Stop();
                _refreshTimer.Interval = TimeSpan.FromMinutes(_refreshRateMinutes);
                _refreshTimer.Start();
                System.Diagnostics.Debug.WriteLine($"🔄 Refresh timer updated: {_refreshRateMinutes} minutes");
            }
        }

        private void OnOpenSettings()
        {
            System.Diagnostics.Debug.WriteLine("⚙️ Open settings requested");
        }

        private void ApplyFilter()
        {
            System.Diagnostics.Debug.WriteLine($"🔄 Applying filter: {SelectedFilter?.ToString() ?? "All"}");

            FilteredFavorites.Clear();

            var filtered = SelectedFilter == null
                ? Favorites
                : Favorites.Where(f => f.CurrentWeather?.Condition == SelectedFilter);

            foreach (var city in filtered)
            {
                FilteredFavorites.Add(city);
            }

            System.Diagnostics.Debug.WriteLine($"   ✅ Filter applied - {FilteredFavorites.Count} cities");
        }

        private string GetPersianPeriodName(CircadianPeriod period)
        {
            return period switch
            {
                CircadianPeriod.DeepNight => "Midnight",
                CircadianPeriod.LateNight => "After night",
                CircadianPeriod.PreDawn => "Before dawn",
                CircadianPeriod.Dawn => "Dawn",
                CircadianPeriod.EarlyMorning => "Early morning",
                CircadianPeriod.LateMorning => "Late morning",
                CircadianPeriod.Noon => "Noon",
                CircadianPeriod.Afternoon => "Afternoon",
                CircadianPeriod.LateAfternoon => "Evening",
                CircadianPeriod.Dusk => "Dusk",
                CircadianPeriod.Evening => "Evening",
                CircadianPeriod.Night => "Night",
                _ => "unspecified"
            };
        }

        private async Task InitializeAsync()
        {
            IsInitializing = true;
            StatusMessage = "Loading your preferences...";

            try
            {
                var settings = await _settingsService.LoadAsync();
                System.Diagnostics.Debug.WriteLine($"📥 Settings loaded from disk:");
                System.Diagnostics.Debug.WriteLine($"   EnableWeatherFilters: {settings.Filters.EnableWeatherFilters}");
                ApplySettingsToUI(settings);

                StatusMessage = "Loading your favorites...";
                var savedCities = await _storageService.LoadFavoritesAsync();

                if (savedCities == null || savedCities.Count == 0)
                {
                    StatusMessage = "No favorites yet. Search for a city!";
                    UpdateRefreshTimer();
                    return;
                }

                foreach (var city in savedCities)
                {
                    try
                    {
                        var viewModel = new CityWeatherViewModel(city, _weatherService, _animationService);
                        Favorites.Add(viewModel);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Failed to load city {city.Name}: {ex.Message}");
                    }
                }

                ApplyFilter();
                StatusMessage = $"Loaded {Favorites.Count} favorites";
                LastUpdateTime = DateTime.Now;
                UpdateRefreshTimer();

                System.Diagnostics.Debug.WriteLine($"✅ InitializeAsync completed - UI State:");
                System.Diagnostics.Debug.WriteLine($"   EnableWeatherFilters: {EnableWeatherFilters}");
                System.Diagnostics.Debug.WriteLine($"   IsFilterBarVisible: {IsFilterBarVisible}");
                System.Diagnostics.Debug.WriteLine($"   CardsGridRow: {CardsGridRow}, RowSpan: {CardsGridRowSpan}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ InitializeAsync failed: {ex}");
            }
            finally
            {
                IsInitializing = false;
                await Task.Delay(3000);
                StatusMessage = null;
            }
        }

        private void ApplySettingsToUI(SettingsModel settings)
        {
            System.Diagnostics.Debug.WriteLine("🔄 Applying settings to UI...");
            System.Diagnostics.Debug.WriteLine(
                $"   Settings.Filters.EnableWeatherFilters: {settings.Filters.EnableWeatherFilters}");

            RefreshRateMinutes = settings.RefreshRateMinutes;

            ShowCurrentWeatherCard = settings.DetailView.ShowCurrentWeatherCard;
            ShowForecastCard = settings.DetailView.ShowForecastCard;
            ShowSunTimesCard = settings.DetailView.ShowSunTimesCard;
            ShowHourlyChart = settings.DetailView.ShowHourlyChart;

            ShowTemperature = settings.CityCard.ShowTemperature;
            ShowHumidity = settings.CityCard.ShowHumidity;
            ShowWindSpeed = settings.CityCard.ShowWindSpeed;
            ShowPressure = settings.CityCard.ShowPressure;

            System.Diagnostics.Debug.WriteLine(
                $"🎯 About to set EnableWeatherFilters to: {settings.Filters.EnableWeatherFilters}");
            EnableWeatherFilters = settings.Filters.EnableWeatherFilters;

            System.Diagnostics.Debug.WriteLine($"✅ Settings applied - Final state:");
            System.Diagnostics.Debug.WriteLine($"   EnableWeatherFilters: {EnableWeatherFilters}");
            System.Diagnostics.Debug.WriteLine($"   IsFilterBarVisible: {IsFilterBarVisible}");
            System.Diagnostics.Debug.WriteLine($"   CardsGridRow: {CardsGridRow}");
            System.Diagnostics.Debug.WriteLine($"   CardsGridRowSpan: {CardsGridRowSpan}");
        }

        private async Task SearchCitiesAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                SearchResults.Clear();
                HasSearchResults = false;
                return;
            }

            _searchCts?.Cancel();
            _searchCts = new System.Threading.CancellationTokenSource();
            var token = _searchCts.Token;

            IsSearching = true;
            try
            {
                await Task.Delay(300, token);
                if (token.IsCancellationRequested) return;

                var (conditionFilter, cityQuery) = ParseSearchQuery(SearchQuery);

                if (string.IsNullOrWhiteSpace(cityQuery))
                {
                    SearchResults.Clear();
                    HasSearchResults = false;
                    return;
                }

                var results = await _weatherService.SearchCitiesAsync(cityQuery);
                SearchResults.Clear();

                var filteredResults = results
                    .Where(c => !Favorites.Any(f => f.City.Id == c.Id))
                    .Take(10);

                if (conditionFilter.HasValue)
                {
                    var resultsWithWeather = new List<(City city, CurrentWeather weather)>();

                    foreach (var city in filteredResults)
                    {
                        try
                        {
                            var weather = await _weatherService.GetCurrentWeatherAsync(city.Lat, city.Lon);
                            if (weather.Condition == conditionFilter.Value)
                            {
                                resultsWithWeather.Add((city, weather));
                            }
                        }
                        catch
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠️ Could not fetch weather for {city.Name}");
                        }
                    }

                    foreach (var (city, _) in resultsWithWeather)
                    {
                        SearchResults.Add(city);
                    }

                    StatusMessage = $"Found {SearchResults.Count} {conditionFilter} cities matching '{cityQuery}'";
                }
                else
                {
                    foreach (var city in filteredResults)
                    {
                        SearchResults.Add(city);
                    }
                }

                HasSearchResults = SearchResults.Count > 0;
                System.Diagnostics.Debug.WriteLine(
                    $"🔍 Search for '{SearchQuery}': {SearchResults.Count} results" +
                    (conditionFilter.HasValue ? $" (filtered by {conditionFilter})" : ""));
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                StatusMessage = $"Search failed: {ex.Message}";
                HasSearchResults = false;
            }
            finally
            {
                IsSearching = false;
            }
        }

        private (WeatherCondition? condition, string query) ParseSearchQuery(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return (null, null);

            var trimmed = input.Trim();
            if (trimmed.Contains(":"))
            {
                var parts = trimmed.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var conditionStr = parts[0].Trim().ToLower();
                    var cityQuery = parts[1].Trim();

                    var condition = conditionStr switch
                    {
                        "sunny" => WeatherCondition.Sunny,
                        "cloudy" => WeatherCondition.Cloudy,
                        "rainy" => WeatherCondition.Rainy,
                        "snowy" => WeatherCondition.Snowy,
                        "storm" => WeatherCondition.Storm,
                        "partly" or "partlycloudy" => WeatherCondition.PartlyCloudy,
                        "foggy" or "fog" => WeatherCondition.Foggy,
                        _ => (WeatherCondition?)null
                    };

                    if (condition.HasValue)
                    {
                        return (condition, cityQuery);
                    }
                }
            }

            return (null, trimmed);
        }

        private void ClearSearch()
        {
            SearchQuery = string.Empty;
            SearchResults.Clear();
            HasSearchResults = false;
            System.Diagnostics.Debug.WriteLine("🔍 Search cleared");
        }

        private async Task AddCityAsync(City city)
        {
            if (city == null || Favorites.Any(f => f.City.Id == city.Id)) return;

            IsSearching = true;
            StatusMessage = $"Adding {city.Name}...";

            try
            {
                await _storageService.AddFavoriteAsync(city);
                var viewModel = new CityWeatherViewModel(city, _weatherService, _animationService);
                Favorites.Add(viewModel);
                ClearSearch();
                StatusMessage = $"{city.Name} added successfully!";
                LastUpdateTime = DateTime.Now;
                ApplyFilter();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to add {city.Name}: {ex.Message}";
            }
            finally
            {
                IsSearching = false;
                await Task.Delay(2000);
                StatusMessage = null;
            }
        }

        private async Task RemoveCityAsync(CityWeatherViewModel cityViewModel)
        {
            if (cityViewModel == null)
                return;

            System.Diagnostics.Debug.WriteLine($"🗑️ Removing city: {cityViewModel.City.Name}");

            try
            {
                Favorites.Remove(cityViewModel);
                System.Diagnostics.Debug.WriteLine($"   ✅ Removed from Favorites collection");
                if (SelectedCity == cityViewModel)
                {
                    SelectedCity = null;
                    System.Diagnostics.Debug.WriteLine($"   ✅ Cleared SelectedCity");
                }

                if (FilteredFavorites.Contains(cityViewModel))
                {
                    FilteredFavorites.Remove(cityViewModel);
                    System.Diagnostics.Debug.WriteLine($"   ✅ Removed from FilteredFavorites collection");
                }

                await _storageService.RemoveFavoriteAsync(cityViewModel.City.Id);
                System.Diagnostics.Debug.WriteLine($"   ✅ Removed from storage");

                StatusMessage = $"{cityViewModel.City.Name} removed";
                System.Diagnostics.Debug.WriteLine($"   ✅ Set status message");

                await Task.Delay(2000);
                StatusMessage = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ RemoveCityAsync failed: {ex}");
                StatusMessage = $"Failed to remove city: {ex.Message}";
                if (!Favorites.Contains(cityViewModel))
                {
                    Favorites.Add(cityViewModel);
                    ApplyFilter();
                    System.Diagnostics.Debug.WriteLine($"   ⚠️ Re-added city due to error");
                }
            }
        }

        public async Task RefreshAllAsync()
        {
            if (!Favorites.Any()) return;

            IsSearching = true;
            StatusMessage = "Refreshing all cities...";
            ApplyFilter();

            try
            {
                var tasks = Favorites.Select(f => f.LoadWeatherAsync()).ToArray();
                await Task.WhenAll(tasks);
                LastUpdateTime = DateTime.Now;
                StatusMessage = "All cities updated!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Refresh failed: {ex.Message}";
            }
            finally
            {
                IsSearching = false;
                await Task.Delay(2000);
                StatusMessage = null;
            }
        }
    }
}