using System;
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
        private IServiceProvider _serviceProvider;
        
        public IServiceProvider ServiceProvider => _serviceProvider;
        
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public override void OnFrameworkInitializationCompleted()
        {
            base.OnFrameworkInitializationCompleted();
    
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
    
            var configService = _serviceProvider.GetRequiredService<IConfigurationService>();
            configService.LoadAsync().Wait();
    
            var circadianService = _serviceProvider.GetRequiredService<ICircadianThemeService>();
            circadianService.StartDynamicTheming();
    
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                desktop.MainWindow = mainWindow;
            }
    
            System.Diagnostics.Debug.WriteLine("✅ Application initialized successfully");
        }
        
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddSingleton<IAnimationService, AnimationService>();
            services.AddSingleton<IStorageService, StorageService>();
            services.AddSingleton<ICircadianThemeService, CircadianThemeService>();
            services.AddSingleton<IWeatherApiService>(sp => 
                new WeatherApiService("<YOUR-API-KEY>", 
                sp.GetRequiredService<ICacheService>()));
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainWindow>();
            
            System.Diagnostics.Debug.WriteLine("✅ Services configured");
        }
    }
}