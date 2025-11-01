using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Weather.Dashboard.Avalonia.Models;
using Weather.Dashboard.Avalonia.Services.Interfaces;
using Polly;
using Polly.Retry;

namespace Weather.Dashboard.Avalonia.Services
{
    public class WeatherApiService : IWeatherApiService
    {
        private readonly string _apiKey;
        private readonly ICacheService _cacheService;
        private readonly HttpClient _httpClient;
        private readonly AsyncRetryPolicy<string> _retryPolicy;

        private const string BaseUrl = "https://api.openweathermap.org/data/2.5";
        private const string OneCallUrl = "https://api.openweathermap.org/data/3.0/onecall";
        private const string GeoUrl = "http://api.openweathermap.org/geo/1.0";

        public WeatherApiService(string apiKey, ICacheService cacheService)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));

            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            _retryPolicy = Policy<string>
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"🔄 Retry #{retryCount} after {timespan.TotalSeconds}s delay");
                    });
        }
        public async Task<CurrentWeather> GetCurrentWeatherAsync(double lat, double lon)
        {
            string cacheKey = $"current_{lat:F2}_{lon:F2}";

            var cached = await _cacheService.GetAsync<CurrentWeather>(cacheKey);
            if (cached != null)
            {
                System.Diagnostics.Debug.WriteLine($"📦 Cache hit for current weather at ({lat}, {lon})");
                return cached;
            }

            try
            {
                var url = $"{BaseUrl}/weather?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";
                var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetStringAsync(url));
                var data = JsonSerializer.Deserialize<JsonElement>(response);

                var weather = new CurrentWeather
                {
                    Temperature = data.GetProperty("main").GetProperty("temp").GetDouble(),
                    FeelsLike = data.GetProperty("main").GetProperty("feels_like").GetDouble(),
                    Humidity = data.GetProperty("main").GetProperty("humidity").GetInt32(),
                    Pressure = data.GetProperty("main").GetProperty("pressure").GetInt32(),
                    WindSpeed = data.GetProperty("wind").GetProperty("speed").GetDouble(),
                    Description =
                        CapitalizeFirstLetter(data.GetProperty("weather")[0].GetProperty("description").GetString()),
                    IconCode = data.GetProperty("weather")[0].GetProperty("icon").GetString(),
                    Condition = MapCondition(data.GetProperty("weather")[0].GetProperty("main").GetString()),
                    Timestamp = DateTime.Now,
                    Visibility = data.TryGetProperty("visibility", out var vis) ? vis.GetDouble() / 1000.0 : 10.0,
                    Sunrise = data.TryGetProperty("sys", out var sys) && sys.TryGetProperty("sunrise", out var sunrise)
                        ? (DateTime?)DateTimeOffset.FromUnixTimeSeconds(sunrise.GetInt64()).LocalDateTime
                        : null,
                    Sunset = data.TryGetProperty("sys", out var sys2) && sys2.TryGetProperty("sunset", out var sunset)
                        ? (DateTime?)DateTimeOffset.FromUnixTimeSeconds(sunset.GetInt64()).LocalDateTime
                        : null
                };

                await _cacheService.SetAsync(cacheKey, weather, 10);
                System.Diagnostics.Debug.WriteLine(
                    $"✅ Current weather loaded: {weather.Temperature}°C, {weather.Description}");

                return weather;
            }
            catch (WeatherApiException ex)
            {
                string ErrorMessage = "⚠️ Cannot connect to weather service. Check your internet or DNS settings.";
                System.Diagnostics.Debug.WriteLine(
                    $"❌ WeatherApiException: {ex.Message} : Error Message = {ErrorMessage}");
                return null;
            }
            catch (HttpRequestException ex)
            {
                string ErrorMessage = "⚠️ Network error. Please check your connection.";
                System.Diagnostics.Debug.WriteLine(
                    $"❌ HttpRequestException: {ex.Message} : Error Message = {ErrorMessage}");
                return null;
            }
            catch (Exception ex)
            {
                string ErrorMessage = $"❌ Unexpected error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ General Exception: {ex} : Error Message = {ErrorMessage}");
                return null;
            }
        }
        public async Task<ForecastItem[]> GetForecastAsync(double lat, double lon, int days = 7)
        {
            string cacheKey = $"forecast_{lat:F2}_{lon:F2}_{days}";
            var cached = await _cacheService.GetAsync<ForecastItem[]>(cacheKey);
            if (cached != null)
            {
                System.Diagnostics.Debug.WriteLine($"📦 Cache hit for forecast at ({lat}, {lon})");
                return cached;
            }

            try
            {
                try
                {
                    var oneCallUrl =
                        $"{OneCallUrl}?lat={lat}&lon={lon}&exclude=minutely,alerts&appid={_apiKey}&units=metric";
                    var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetStringAsync(oneCallUrl));
                    var data = JsonSerializer.Deserialize<JsonElement>(response);
                    var forecast = data.GetProperty("daily")
                        .EnumerateArray()
                        .Skip(1) 
                        .Take(days) 
                        .Select(item => new ForecastItem
                        {
                            DateTime = DateTimeOffset.FromUnixTimeSeconds(item.GetProperty("dt").GetInt64()).DateTime,
                            MinTemp = item.GetProperty("temp").GetProperty("min").GetDouble(),
                            MaxTemp = item.GetProperty("temp").GetProperty("max").GetDouble(),
                            Condition = MapCondition(item.GetProperty("weather")[0].GetProperty("main").GetString()),
                            PrecipitationProb = (int)(item.GetProperty("pop").GetDouble() * 100),
                            Humidity = item.GetProperty("humidity").GetDouble(),
                            WindSpeed = item.GetProperty("wind_speed").GetDouble(),
                            IconCode = item.GetProperty("weather")[0].GetProperty("icon").GetString()
                        })
                        .ToArray();

                    await _cacheService.SetAsync(cacheKey, forecast, 30);
                    System.Diagnostics.Debug.WriteLine(
                        $"✅ Forecast loaded (One Call API): {forecast.Length} days (excluding today)");
                    return forecast;
                }
                catch
                {
                    return await GetForecastFallbackAsync(lat, lon, days, cacheKey);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to load forecast: {ex.Message}");
                throw new WeatherApiException("Failed to load forecast data. Please check your internet connection.",
                    ex);
            }
        }

        private async Task<ForecastItem[]> GetForecastFallbackAsync(double lat, double lon, int days, string cacheKey)
        {
            var url = $"{BaseUrl}/forecast?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";
            var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetStringAsync(url));
            var data = JsonSerializer.Deserialize<JsonElement>(response);

            var today = DateTime.Now.Date;
            var grouped = data.GetProperty("list")
                .EnumerateArray()
                .GroupBy(item =>
                {
                    var itemDate = DateTimeOffset.FromUnixTimeSeconds(item.GetProperty("dt").GetInt64()).Date;
                    return itemDate;
                })
                .Where(group => group.Key > today)
                .Take(days)
                .Select(group =>
                {
                    var temps = group.Select(g => g.GetProperty("main").GetProperty("temp").GetDouble()).ToList();
                    var firstItem = group.First();
                    return new ForecastItem
                    {
                        DateTime = group.Key,
                        MinTemp = temps.Min(),
                        MaxTemp = temps.Max(),
                        Condition = MapCondition(firstItem.GetProperty("weather")[0].GetProperty("main").GetString()),
                        PrecipitationProb = (int)(group.Average(g => g.GetProperty("pop").GetDouble()) * 100),
                        Humidity = group.Average(g => g.GetProperty("main").GetProperty("humidity").GetDouble()),
                        WindSpeed = group.Average(g => g.GetProperty("wind").GetProperty("speed").GetDouble()),
                        IconCode = firstItem.GetProperty("weather")[0].GetProperty("icon").GetString()
                    };
                })
                .ToArray();

            await _cacheService.SetAsync(cacheKey, grouped, 30);
            System.Diagnostics.Debug.WriteLine(
                $"✅ Forecast loaded (5-day API fallback): {grouped.Length} days (excluding today)");
            return grouped;
        }

        public async Task<double[]> GetHourlyTemperaturesAsync(double lat, double lon)
        {
            string cacheKey = $"hourly_{lat:F2}_{lon:F2}";

            var cached = await _cacheService.GetAsync<double[]>(cacheKey);
            if (cached != null)
            {
                System.Diagnostics.Debug.WriteLine($"📦 Cache hit for hourly temps at ({lat}, {lon})");
                return cached;
            }

            try
            {
                try
                {
                    var url =
                        $"{OneCallUrl}?lat={lat}&lon={lon}&exclude=daily,minutely,alerts&appid={_apiKey}&units=metric";
                    var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetStringAsync(url));
                    var data = JsonSerializer.Deserialize<JsonElement>(response);

                    var hourlyTemps = data.GetProperty("hourly").EnumerateArray()
                        .Take(24)
                        .Select(item => item.GetProperty("temp").GetDouble())
                        .ToArray();

                    await _cacheService.SetAsync(cacheKey, hourlyTemps, 15);
                    System.Diagnostics.Debug.WriteLine(
                        $"✅ Hourly temps loaded (One Call API): {hourlyTemps.Length} hours");

                    return hourlyTemps;
                }
                catch
                {
                    var url = $"{BaseUrl}/forecast?lat={lat}&lon={lon}&appid={_apiKey}&units=metric";
                    var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetStringAsync(url));
                    var data = JsonSerializer.Deserialize<JsonElement>(response);

                    var threeHourTemps = data.GetProperty("list").EnumerateArray()
                        .Take(8)
                        .Select(item => item.GetProperty("main").GetProperty("temp").GetDouble())
                        .ToArray();
                    var interpolated = InterpolateHourlyData(threeHourTemps);
                    await _cacheService.SetAsync(cacheKey, interpolated, 15);
                    System.Diagnostics.Debug.WriteLine(
                        $"✅ Hourly temps loaded (5-day API, interpolated): {interpolated.Length} hours");

                    return interpolated;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to load hourly temperatures: {ex.Message}");
                throw new WeatherApiException(
                    "Failed to load hourly temperature data. Please check your internet connection.", ex);
            }
        }
        public async Task<City[]> SearchCitiesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Array.Empty<City>();

            try
            {
                var url = $"{GeoUrl}/direct?q={Uri.EscapeDataString(query)}&limit=10&appid={_apiKey}";
                var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetStringAsync(url));
                var data = JsonSerializer.Deserialize<JsonElement>(response);

                var cities = data.EnumerateArray().Select(item => new City(
                    item.GetProperty("name").GetString(),
                    item.TryGetProperty("country", out var country) ? country.GetString() : "N/A",
                    item.GetProperty("lat").GetDouble(),
                    item.GetProperty("lon").GetDouble()
                )).ToArray();

                System.Diagnostics.Debug.WriteLine($"✅ City search for '{query}': {cities.Length} results");
                return cities;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ City search failed: {ex.Message}");
                throw new WeatherApiException(
                    $"Failed to search for cities matching '{query}'. Please check your internet connection.", ex);
            }
        }
        private double[] InterpolateHourlyData(double[] threeHourData)
        {
            if (threeHourData.Length < 2)
                return threeHourData;

            var result = new double[24];

            for (int i = 0; i < threeHourData.Length - 1; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int index = i * 3 + j;
                    if (index < 24)
                    {
                        double t = j / 3.0;
                        result[index] = threeHourData[i] + (threeHourData[i + 1] - threeHourData[i]) * t;
                    }
                }
            }
            if (result[23] == 0 && threeHourData.Length > 0)
            {
                result[23] = threeHourData[threeHourData.Length - 1];
            }

            return result;
        }
        private WeatherCondition MapCondition(string main)
        {
            return main?.ToLower() switch
            {
                "clear" => WeatherCondition.Sunny,
                "clouds" => WeatherCondition.Cloudy,
                "rain" => WeatherCondition.Rainy,
                "drizzle" => WeatherCondition.Rainy,
                "snow" => WeatherCondition.Snowy,
                "thunderstorm" => WeatherCondition.Storm,
                "mist" => WeatherCondition.Foggy,
                "fog" => WeatherCondition.Foggy,
                "haze" => WeatherCondition.Foggy,
                "smoke" => WeatherCondition.Foggy,
                "dust" => WeatherCondition.Foggy,
                "sand" => WeatherCondition.Foggy,
                "ash" => WeatherCondition.Foggy,
                "squall" => WeatherCondition.Storm,
                "tornado" => WeatherCondition.Storm,
                _ => WeatherCondition.PartlyCloudy
            };
        }
        private string CapitalizeFirstLetter(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (text.Length == 1)
                return text.ToUpper();

            return char.ToUpper(text[0]) + text.Substring(1);
        }
    }
    public class WeatherApiException : Exception
    {
        public WeatherApiException(string message) : base(message)
        {
        }

        public WeatherApiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}