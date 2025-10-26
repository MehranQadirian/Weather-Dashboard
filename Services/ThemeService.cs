using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Weather.Dashboard.Avalonia.Services
{
    public interface IThemeService
    {
        bool IsDarkTheme { get; }
        event EventHandler<bool> ThemeChanged;
        void ToggleTheme();
        void ApplyTheme(bool isDarkTheme);
        void LoadTheme();
        void SaveTheme(bool isDarkTheme);
    }

    public class ThemeService : IThemeService
    {
        private bool _isDarkTheme;
        private readonly IConfigurationService _configService;

        public bool IsDarkTheme => _isDarkTheme;
        public event EventHandler<bool> ThemeChanged;

        public ThemeService(IConfigurationService configService)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        }

        public void LoadTheme()
        {
            _isDarkTheme = _configService.Configuration.IsDarkTheme;
            if (Application.Current != null)
            {
                ApplyTheme(_isDarkTheme);
            }
            System.Diagnostics.Debug.WriteLine($"✅ Theme loaded: {(_isDarkTheme ? "Dark" : "Light")}");
        }

        public void ToggleTheme()
        {
            _isDarkTheme = !_isDarkTheme;
            _configService.Configuration.IsDarkTheme = _isDarkTheme;
            _ = _configService.SaveAsync();

            if (Application.Current != null)
            {
                ApplyTheme(_isDarkTheme);
            }

            ThemeChanged?.Invoke(this, _isDarkTheme);
            System.Diagnostics.Debug.WriteLine($"🎨 Theme toggled to: {(_isDarkTheme ? "Dark" : "Light")}");
        }

        public void ApplyTheme(bool isDarkTheme)
        {
            var app = Application.Current;
            if (app?.Resources == null)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Application resources not available");
                return;
            }

            var resources = app.Resources;

            if (isDarkTheme)
            {
                ApplyDarkTheme(resources);
            }
            else
            {
                ApplyLightTheme(resources);
            }

            System.Diagnostics.Debug.WriteLine($"✅ Theme applied: {(isDarkTheme ? "Dark" : "Light")}");
        }

        private void ApplyLightTheme(IResourceDictionary resources)
        {
            resources["PrimaryBrush"] = new SolidColorBrush(Color.Parse("#2563EB"));
            resources["OnPrimaryBrush"] = new SolidColorBrush(Color.Parse("#FFFFFF"));
            resources["PrimaryContainerBrush"] = new SolidColorBrush(Color.Parse("#DBEAFE"));
            resources["OnPrimaryContainerBrush"] = new SolidColorBrush(Color.Parse("#1E3A8A"));
            
            resources["SecondaryBrush"] = new SolidColorBrush(Color.Parse("#7C3AED"));
            resources["OnSecondaryBrush"] = new SolidColorBrush(Color.Parse("#FFFFFF"));
            resources["SecondaryContainerBrush"] = new SolidColorBrush(Color.Parse("#EDE9FE"));
            
            resources["SurfaceBrush"] = new SolidColorBrush(Color.Parse("#FFFFFF"));
            resources["OnSurfaceBrush"] = new SolidColorBrush(Color.Parse("#0F172A"));
            resources["SurfaceVariantBrush"] = new SolidColorBrush(Color.Parse("#F8FAFC"));
            resources["OnSurfaceVariantBrush"] = new SolidColorBrush(Color.Parse("#64748B"));
            
            resources["BackgroundBrush"] = new SolidColorBrush(Color.Parse("#F1F5F9"));
            resources["OnBackgroundBrush"] = new SolidColorBrush(Color.Parse("#0F172A"));
            
            resources["OutlineBrush"] = new SolidColorBrush(Color.Parse("#E2E8F0"));
            resources["OutlineVariantBrush"] = new SolidColorBrush(Color.Parse("#CBD5E1"));
            
            resources["ErrorBrush"] = new SolidColorBrush(Color.Parse("#EF4444"));
            resources["SuccessBrush"] = new SolidColorBrush(Color.Parse("#10B981"));
            resources["WarningBrush"] = new SolidColorBrush(Color.Parse("#F59E0B"));
            resources["InfoBrush"] = new SolidColorBrush(Color.Parse("#3B82F6"));
            
            resources["TextPrimaryBrush"] = new SolidColorBrush(Color.Parse("#0F172A"));
            resources["TextSecondaryBrush"] = new SolidColorBrush(Color.Parse("#64748B"));
        }

        private void ApplyDarkTheme(IResourceDictionary resources)
        {
            resources["PrimaryBrush"] = new SolidColorBrush(Color.Parse("#60A5FA"));
            resources["OnPrimaryBrush"] = new SolidColorBrush(Color.Parse("#1E3A8A"));
            resources["PrimaryContainerBrush"] = new SolidColorBrush(Color.Parse("#1E40AF"));
            resources["OnPrimaryContainerBrush"] = new SolidColorBrush(Color.Parse("#DBEAFE"));
            
            resources["SecondaryBrush"] = new SolidColorBrush(Color.Parse("#A78BFA"));
            resources["OnSecondaryBrush"] = new SolidColorBrush(Color.Parse("#4C1D95"));
            resources["SecondaryContainerBrush"] = new SolidColorBrush(Color.Parse("#5B21B6"));
            
            resources["SurfaceBrush"] = new SolidColorBrush(Color.Parse("#1E293B"));
            resources["OnSurfaceBrush"] = new SolidColorBrush(Color.Parse("#F1F5F9"));
            resources["SurfaceVariantBrush"] = new SolidColorBrush(Color.Parse("#1A1F28"));
            resources["OnSurfaceVariantBrush"] = new SolidColorBrush(Color.Parse("#B0BCC4"));
            
            resources["BackgroundBrush"] = new SolidColorBrush(Color.Parse("#020617"));
            resources["OnBackgroundBrush"] = new SolidColorBrush(Color.Parse("#F8FAFC"));
            
            resources["OutlineBrush"] = new SolidColorBrush(Color.Parse("#334155"));
            resources["OutlineVariantBrush"] = new SolidColorBrush(Color.Parse("#475569"));
            
            resources["ErrorBrush"] = new SolidColorBrush(Color.Parse("#F87171"));
            resources["SuccessBrush"] = new SolidColorBrush(Color.Parse("#34D399"));
            resources["WarningBrush"] = new SolidColorBrush(Color.Parse("#FBBF24"));
            resources["InfoBrush"] = new SolidColorBrush(Color.Parse("#60A5FA"));
            
            resources["TextPrimaryBrush"] = new SolidColorBrush(Color.Parse("#F1F5F9"));
            resources["TextSecondaryBrush"] = new SolidColorBrush(Color.Parse("#B0BCC4"));
        }

        public void SaveTheme(bool isDarkTheme)
        {
            _isDarkTheme = isDarkTheme;
            _configService.Configuration.IsDarkTheme = isDarkTheme;
            _ = _configService.SaveAsync();
            ApplyTheme(_isDarkTheme);
            ThemeChanged?.Invoke(this, _isDarkTheme);
            System.Diagnostics.Debug.WriteLine($"💾 Theme saved: {(_isDarkTheme ? "Dark" : "Light")}");
        }
    }
}