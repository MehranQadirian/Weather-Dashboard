using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Weather.Dashboard.Avalonia.Models;

namespace Weather.Dashboard.Avalonia.Services
{
    public interface IStorageService
    {
        Task<List<City>> LoadFavoritesAsync();
        Task SaveFavoritesAsync(List<City> cities);
        Task AddFavoriteAsync(City city);
        Task RemoveFavoriteAsync(string cityId);
    }

    public class StorageService : IStorageService
    {
        private readonly string _favoritesPath;
        private List<City> _cachedFavorites;

        public StorageService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appData, "WeatherDashboard");
            Directory.CreateDirectory(appFolder);
            _favoritesPath = Path.Combine(appFolder, "favorites.json");
        }

        public async Task<List<City>> LoadFavoritesAsync()
        {
            if (_cachedFavorites != null)
                return new List<City>(_cachedFavorites);

            return await Task.Run(() =>
            {
                try
                {
                    if (File.Exists(_favoritesPath))
                    {
                        var json = File.ReadAllText(_favoritesPath);
                        var favorites = JsonSerializer.Deserialize<List<CityDto>>(json);
                        _cachedFavorites = favorites?.Select(dto => new City(
                            dto.Name,
                            dto.Country,
                            dto.Lat,
                            dto.Lon
                        )).ToList() ?? new List<City>();
                    }
                    else
                    {
                        _cachedFavorites = GetDefaultCities();
                        SaveFavoritesAsync(_cachedFavorites).Wait();
                    }
                }
                catch
                {
                    _cachedFavorites = GetDefaultCities();
                }

                return new List<City>(_cachedFavorites);
            });
        }

        public async Task SaveFavoritesAsync(List<City> cities)
        {
            await Task.Run(() =>
            {
                try
                {
                    _cachedFavorites = new List<City>(cities);
                    var dtos = cities.Select(c => new CityDto
                    {
                        Name = c.Name,
                        Country = c.Country,
                        Lat = c.Lat,
                        Lon = c.Lon
                    }).ToList();

                    var json = JsonSerializer.Serialize(dtos, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    File.WriteAllText(_favoritesPath, json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to save favorites: {ex.Message}");
                }
            });
        }

        public async Task AddFavoriteAsync(City city)
        {
            var favorites = await LoadFavoritesAsync();
            if (!favorites.Any(c => c.Id == city.Id))
            {
                favorites.Add(city);
                await SaveFavoritesAsync(favorites);
            }
        }

        public async Task RemoveFavoriteAsync(string cityId)
        {
            var favorites = await LoadFavoritesAsync();
            var city = favorites.FirstOrDefault(c => c.Id == cityId);
            if (city != null)
            {
                favorites.Remove(city);
                await SaveFavoritesAsync(favorites);
            }
        }

        private List<City> GetDefaultCities()
        {
            return new List<City>
            {
                new City("London", "GB", 51.5074, -0.1278),
                new City("Paris", "FR", 48.8566, 2.3522),
                new City("New York", "US", 40.7128, -74.0060)
            };
        }

        private class CityDto
        {
            public string Name { get; set; }
            public string Country { get; set; }
            public double Lat { get; set; }
            public double Lon { get; set; }
        }
    }
}