using System.Threading.Tasks;

namespace Weather.Dashboard.Avalonia.Services.Interfaces
{
    public interface ICacheService
    {
        Task<T> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, int ttlMinutes) where T : class;
        Task<bool> ExistsAsync(string key);
        Task RemoveAsync(string key);
        Task ClearAsync();
    }
}