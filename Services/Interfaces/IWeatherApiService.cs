using System.Threading.Tasks;
using Weather.Dashboard.Avalonia.Models;

namespace Weather.Dashboard.Avalonia.Services.Interfaces
{
    public interface IWeatherApiService
    {
        Task<CurrentWeather> GetCurrentWeatherAsync(double lat, double lon);
        Task<ForecastItem[]> GetForecastAsync(double lat, double lon, int days = 7);
        Task<City[]> SearchCitiesAsync(string query);
        Task<double[]> GetHourlyTemperaturesAsync(double lat, double lon);
    }
}