using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Weather.Dashboard.Avalonia.Services;
using Weather.Dashboard.Avalonia.Services.Interfaces;
using Weather.Dashboard.Avalonia.ViewModels;

namespace Weather.Dashboard.Avalonia
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;
        
        public IServiceProvider? ServiceProvider => _serviceProvider;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            base.OnFrameworkInitializationCompleted();

            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            try
            {
                var configService = _serviceProvider.GetRequiredService<IConfigurationService>();
                await configService.LoadAsync();

                var settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
                var settings = await settingsService.LoadAsync();

                var circadianService = _serviceProvider.GetRequiredService<ICircadianThemeService>();

                if (settings.Theme.EnableDynamicTheme)
                {
                    Debug.WriteLine("🌓 Starting dynamic (circadian) theme");
                    circadianService.StartDynamicTheming();
                }
                else if (settings.Theme.FixedThemeIndex >= 0)
                {
                    Debug.WriteLine($"🎨 Applying fixed theme: {settings.Theme.FixedThemeIndex}");
                    circadianService.StopDynamicTheming();
                    circadianService.ApplyThemeForPeriod((CircadianPeriod)settings.Theme.FixedThemeIndex);
                }
                else
                {
                    Debug.WriteLine("🌓 No theme preference, starting dynamic theme (default)");
                    circadianService.StartDynamicTheming();
                }

                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                    desktop.MainWindow = mainWindow;
                    mainWindow.Show();
                    Debug.WriteLine("✅ MainWindow configured and shown");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ OnFrameworkInitializationCompleted error: {ex.Message}");
                Debug.WriteLine($"   Stack: {ex.StackTrace}");
                throw;
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddSingleton<IAnimationService, AnimationService>();
            services.AddSingleton<IStorageService, StorageService>();
            services.AddSingleton<ICircadianThemeService, CircadianThemeService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IWeatherApiService>(sp => 
                new WeatherApiService(
                    "YOUR_API_KEY",
                    sp.GetRequiredService<ICacheService>()
                )
            );
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainWindow>();

            Debug.WriteLine("✅ Services configured");
        }
    }
}