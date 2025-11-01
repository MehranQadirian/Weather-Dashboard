using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Weather.Dashboard.Avalonia.Services.Interfaces;

namespace Weather.Dashboard.Avalonia.Services
{
    public class CacheService : ICacheService
    {
        private readonly string _cacheDirectory;
        private readonly Dictionary<string, CacheEntry> _memoryCache;

        public CacheService()
        {
            _cacheDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WeatherDashboard",
                "Cache"
            );
            Directory.CreateDirectory(_cacheDirectory);
            _memoryCache = new Dictionary<string, CacheEntry>();
        }

        public Task<T> GetAsync<T>(string key) where T : class
        {
            if (_memoryCache.TryGetValue(key, out var entry))
            {
                if (entry.ExpiresAt > DateTime.Now)
                {
                    return Task.FromResult(entry.Value as T);
                }
                _memoryCache.Remove(key);
            }
            string filePath = GetCacheFilePath(key);
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    var fileEntry = JsonSerializer.Deserialize<CacheEntry>(json);

                    if (fileEntry.ExpiresAt > DateTime.Now)
                    {
                        var value = JsonSerializer.Deserialize<T>(fileEntry.ValueJson);
                        _memoryCache[key] = new CacheEntry
                        {
                            Value = value,
                            ExpiresAt = fileEntry.ExpiresAt
                        };
                        return Task.FromResult(value);
                    }

                    File.Delete(filePath);
                }
                catch
                { }
            }

            return Task.FromResult<T>(null);
        }

        public Task SetAsync<T>(string key, T value, int ttlMinutes) where T : class
        {
            var expiresAt = DateTime.Now.AddMinutes(ttlMinutes);
            _memoryCache[key] = new CacheEntry
            {
                Value = value,
                ExpiresAt = expiresAt
            };
            try
            {
                var entry = new CacheEntry
                {
                    ValueJson = JsonSerializer.Serialize(value),
                    ExpiresAt = expiresAt
                };

                string json = JsonSerializer.Serialize(entry);
                File.WriteAllText(GetCacheFilePath(key), json);
            }
            catch
            { }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            if (_memoryCache.ContainsKey(key) && _memoryCache[key].ExpiresAt > DateTime.Now)
            {
                return Task.FromResult(true);
            }

            string filePath = GetCacheFilePath(key);
            return Task.FromResult(File.Exists(filePath));
        }

        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);

            string filePath = GetCacheFilePath(key);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }

        public Task ClearAsync()
        {
            _memoryCache.Clear();

            if (Directory.Exists(_cacheDirectory))
            {
                Directory.Delete(_cacheDirectory, true);
                Directory.CreateDirectory(_cacheDirectory);
            }

            return Task.CompletedTask;
        }

        private string GetCacheFilePath(string key)
        {
            string safeKey = string.Join("_", key.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(_cacheDirectory, $"{safeKey}.json");
        }

        private class CacheEntry
        {
            public object Value { get; set; }
            public string ValueJson { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }
}
