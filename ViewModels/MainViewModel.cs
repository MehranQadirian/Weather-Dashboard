using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
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
        private System.Threading.CancellationTokenSource _searchCts;
        private string _currentPeriod;

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
            IStorageService storageService)
        {
            _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
            _animationService = animationService ?? throw new ArgumentNullException(nameof(animationService));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

            var circadianService = ((App)Application.Current).ServiceProvider.GetService<ICircadianThemeService>();
            if (circadianService != null)
            {
                circadianService.ThemeChanged += (s, period) =>
                {
                    CurrentPeriod = GetPersianPeriodName(period);
                };
                CurrentPeriod = GetPersianPeriodName(circadianService.GetCurrentPeriod());
            }

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

            _ = InitializeAsync();
        }

        private void ApplyFilter()
        {
            FilteredFavorites.Clear();
            var filtered = SelectedFilter == null
                ? Favorites
                : Favorites.Where(f => f.CurrentWeather?.Condition == SelectedFilter);

            foreach (var city in filtered)
            {
                FilteredFavorites.Add(city);
            }

            System.Diagnostics.Debug.WriteLine(
                $"🔍 Filter applied: {SelectedFilter?.ToString() ?? "All"} - {FilteredFavorites.Count} cities");
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
            StatusMessage = "Loading your favorites...";
            try
            {
                var savedCities = await _storageService.LoadFavoritesAsync();
                if (savedCities == null || savedCities.Count == 0)
                {
                    StatusMessage = "No favorites yet. Search for a city!";
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
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading favorites: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ InitializeAsync failed: {ex}");
            }
            finally
            {
                IsInitializing = false;
                await Task.Delay(3000);
                StatusMessage = null;
            }
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
                if (token.IsCancellationRequested)
                    return;

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
            if (string.IsNullOrWhiteSpace(input))
                return (null, null);

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
        }

        private async Task AddCityAsync(City city)
        {
            if (city == null || Favorites.Any(f => f.City.Id == city.Id))
                return;

            
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

            ApplyFilter();
            try
            {
                await _storageService.RemoveFavoriteAsync(cityViewModel.City.Id);
                Favorites.Remove(cityViewModel);
                if (SelectedCity == cityViewModel)
                {
                    SelectedCity = null;
                }

                StatusMessage = $"{cityViewModel.City.Name} removed";
                await Task.Delay(2000);
                StatusMessage = null;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to remove city: {ex.Message}";
            }
        }

        private async Task RefreshAllAsync()
        {
            if (!Favorites.Any())
                return;

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