using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace Weather.Dashboard.Avalonia.Services
{
    public interface ICircadianThemeService
    {
        event EventHandler<CircadianPeriod> ThemeChanged;
        void StartDynamicTheming();
        void StopDynamicTheming();
        CircadianPeriod GetCurrentPeriod();
        void ApplyThemeForPeriod(CircadianPeriod period);
    }

    public enum CircadianPeriod
    {
        DeepNight = 0,      // 00:00-02:00
        LateNight = 1,      // 02:00-04:00
        PreDawn = 2,        // 04:00-06:00
        Dawn = 3,           // 06:00-08:00
        EarlyMorning = 4,   // 08:00-10:00
        LateMorning = 5,    // 10:00-12:00
        Noon = 6,           // 12:00-14:00
        Afternoon = 7,      // 14:00-16:00
        LateAfternoon = 8,  // 16:00-18:00
        Dusk = 9,           // 18:00-20:00
        Evening = 10,       // 20:00-22:00
        Night = 11          // 22:00-00:00
    }

    public class CircadianThemeService : ICircadianThemeService
    {
        private DispatcherTimer _themeTimer;
        private CircadianPeriod _currentPeriod;

        public event EventHandler<CircadianPeriod> ThemeChanged;

        public CircadianThemeService()
        {
            _themeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _themeTimer.Tick += OnThemeTimerTick;
        }

        public void StartDynamicTheming()
        {
            _currentPeriod = GetCurrentPeriod();
            ApplyThemeForPeriod(_currentPeriod);
            _themeTimer.Start();
            System.Diagnostics.Debug.WriteLine($"🌓 Circadian theming started: {_currentPeriod}");
        }

        public void StopDynamicTheming()
        {
            _themeTimer.Stop();
        }

        public CircadianPeriod GetCurrentPeriod()
        {
            var hour = DateTime.Now.Hour;
            return (CircadianPeriod)(hour / 2);
        }

        private void OnThemeTimerTick(object sender, EventArgs e)
        {
            var newPeriod = GetCurrentPeriod();
            if (newPeriod != _currentPeriod)
            {
                _currentPeriod = newPeriod;
                ApplyThemeForPeriod(_currentPeriod);
                ThemeChanged?.Invoke(this, _currentPeriod);
                System.Diagnostics.Debug.WriteLine($"🌓 Theme auto-switched to: {_currentPeriod}");
            }
        }

        public void ApplyThemeForPeriod(CircadianPeriod period)
        {
            var app = Application.Current;
            if (app?.Resources == null)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Application resources not available");
                return;
            }

            var palette = GetPaletteForPeriod(period);
            ApplyPalette(app.Resources, palette);
            
            System.Diagnostics.Debug.WriteLine($"✅ Applied {period} theme (Hour: {DateTime.Now.Hour})");
        }

        private ColorPalette GetPaletteForPeriod(CircadianPeriod period)
        {
            return period switch
            {
                CircadianPeriod.DeepNight => new ColorPalette
                {
                    Primary = Color.FromRgb(139, 92, 246),
                    OnPrimary = Color.FromRgb(76, 29, 149),
                    PrimaryContainer = Color.FromRgb(59, 33, 108),
                    OnPrimaryContainer = Color.FromRgb(216, 180, 254),
                    
                    Secondary = Color.FromRgb(96, 165, 250),
                    OnSecondary = Color.FromRgb(30, 58, 138),
                    SecondaryContainer = Color.FromRgb(30, 64, 175),
                    
                    Surface = Color.FromRgb(17, 24, 39),
                    OnSurface = Color.FromRgb(229, 231, 235),
                    SurfaceVariant = Color.FromRgb(31, 41, 55),
                    OnSurfaceVariant = Color.FromRgb(209, 213, 219),
                    
                    Background = Color.FromRgb(3, 7, 18),
                    OnBackground = Color.FromRgb(243, 244, 246),
                    
                    Outline = Color.FromRgb(55, 65, 81),
                    OutlineVariant = Color.FromRgb(75, 85, 99),
                    
                    Error = Color.FromRgb(239, 68, 68),
                    Success = Color.FromRgb(34, 197, 94),
                    Warning = Color.FromRgb(251, 146, 60),
                    Info = Color.FromRgb(59, 130, 246),
                    
                    TextPrimary = Color.FromRgb(243, 244, 246),
                    TextSecondary = Color.FromRgb(156, 163, 175)
                },

                CircadianPeriod.LateNight => new ColorPalette
                {
                    Primary = Color.FromRgb(124, 58, 237),
                    OnPrimary = Color.FromRgb(67, 20, 149),
                    PrimaryContainer = Color.FromRgb(76, 29, 149),
                    OnPrimaryContainer = Color.FromRgb(221, 214, 254),
                    
                    Secondary = Color.FromRgb(79, 70, 229),
                    OnSecondary = Color.FromRgb(30, 27, 75),
                    SecondaryContainer = Color.FromRgb(49, 46, 129),
                    
                    Surface = Color.FromRgb(24, 24, 27),
                    OnSurface = Color.FromRgb(228, 228, 231),
                    SurfaceVariant = Color.FromRgb(39, 39, 42),
                    OnSurfaceVariant = Color.FromRgb(212, 212, 216),
                    
                    Background = Color.FromRgb(9, 9, 11),
                    OnBackground = Color.FromRgb(244, 244, 245),
                    
                    Outline = Color.FromRgb(63, 63, 70),
                    OutlineVariant = Color.FromRgb(82, 82, 91),
                    
                    Error = Color.FromRgb(248, 113, 113),
                    Success = Color.FromRgb(52, 211, 153),
                    Warning = Color.FromRgb(251, 191, 36),
                    Info = Color.FromRgb(96, 165, 250),
                    
                    TextPrimary = Color.FromRgb(244, 244, 245),
                    TextSecondary = Color.FromRgb(161, 161, 170)
                },

                CircadianPeriod.PreDawn => new ColorPalette
                {
                    Primary = Color.FromRgb(99, 102, 241),
                    OnPrimary = Color.FromRgb(238, 242, 255),
                    PrimaryContainer = Color.FromRgb(67, 56, 202),
                    OnPrimaryContainer = Color.FromRgb(224, 231, 255),
                    
                    Secondary = Color.FromRgb(147, 51, 234),
                    OnSecondary = Color.FromRgb(250, 245, 255),
                    SecondaryContainer = Color.FromRgb(107, 33, 168),
                    
                    Surface = Color.FromRgb(30, 41, 59),
                    OnSurface = Color.FromRgb(241, 245, 249),
                    SurfaceVariant = Color.FromRgb(51, 65, 85),
                    OnSurfaceVariant = Color.FromRgb(226, 232, 240),
                    
                    Background = Color.FromRgb(15, 23, 42),
                    OnBackground = Color.FromRgb(248, 250, 252),
                    
                    Outline = Color.FromRgb(71, 85, 105),
                    OutlineVariant = Color.FromRgb(100, 116, 139),
                    
                    Error = Color.FromRgb(252, 165, 165),
                    Success = Color.FromRgb(74, 222, 128),
                    Warning = Color.FromRgb(253, 224, 71),
                    Info = Color.FromRgb(125, 211, 252),
                    
                    TextPrimary = Color.FromRgb(248, 250, 252),
                    TextSecondary = Color.FromRgb(203, 213, 225)
                },

                CircadianPeriod.Dawn => new ColorPalette
                {
                    Primary = Color.FromRgb(251, 146, 60),
                    OnPrimary = Color.FromRgb(67, 20, 7),
                    PrimaryContainer = Color.FromRgb(194, 65, 12),
                    OnPrimaryContainer = Color.FromRgb(255, 237, 213),
                    
                    Secondary = Color.FromRgb(236, 72, 153),
                    OnSecondary = Color.FromRgb(80, 7, 36),
                    SecondaryContainer = Color.FromRgb(157, 23, 77),
                    
                    Surface = Color.FromRgb(51, 65, 85),
                    OnSurface = Color.FromRgb(248, 250, 252),
                    SurfaceVariant = Color.FromRgb(71, 85, 105),
                    OnSurfaceVariant = Color.FromRgb(241, 245, 249),
                    
                    Background = Color.FromRgb(30, 41, 59),
                    OnBackground = Color.FromRgb(255, 251, 235),
                    
                    Outline = Color.FromRgb(100, 116, 139),
                    OutlineVariant = Color.FromRgb(148, 163, 184),
                    
                    Error = Color.FromRgb(239, 68, 68),
                    Success = Color.FromRgb(52, 211, 153),
                    Warning = Color.FromRgb(251, 191, 36),
                    Info = Color.FromRgb(59, 130, 246),
                    
                    TextPrimary = Color.FromRgb(255, 251, 235),
                    TextSecondary = Color.FromRgb(254, 215, 170)
                },

                CircadianPeriod.EarlyMorning => new ColorPalette
                {
                    Primary = Color.FromRgb(249, 115, 22),
                    OnPrimary = Color.FromRgb(255, 255, 255),
                    PrimaryContainer = Color.FromRgb(255, 237, 213),
                    OnPrimaryContainer = Color.FromRgb(124, 45, 18),
                    
                    Secondary = Color.FromRgb(234, 88, 12),
                    OnSecondary = Color.FromRgb(255, 255, 255),
                    SecondaryContainer = Color.FromRgb(254, 243, 199),
                    
                    Surface = Color.FromRgb(254, 252, 232),
                    OnSurface = Color.FromRgb(28, 25, 23),
                    SurfaceVariant = Color.FromRgb(255, 251, 235),
                    OnSurfaceVariant = Color.FromRgb(68, 64, 60),
                    
                    Background = Color.FromRgb(255, 248, 225),
                    OnBackground = Color.FromRgb(41, 37, 36),
                    
                    Outline = Color.FromRgb(231, 229, 228),
                    OutlineVariant = Color.FromRgb(214, 211, 209),
                    
                    Error = Color.FromRgb(220, 38, 38),
                    Success = Color.FromRgb(22, 163, 74),
                    Warning = Color.FromRgb(245, 158, 11),
                    Info = Color.FromRgb(37, 99, 235),
                    
                    TextPrimary = Color.FromRgb(28, 25, 23),
                    TextSecondary = Color.FromRgb(87, 83, 78)
                },

                CircadianPeriod.LateMorning => new ColorPalette
                {
                    Primary = Color.FromRgb(251, 191, 36),
                    OnPrimary = Color.FromRgb(255, 255, 255),
                    PrimaryContainer = Color.FromRgb(254, 249, 195),
                    OnPrimaryContainer = Color.FromRgb(113, 63, 18),
                    
                    Secondary = Color.FromRgb(34, 197, 94),
                    OnSecondary = Color.FromRgb(255, 255, 255),
                    SecondaryContainer = Color.FromRgb(220, 252, 231),
                    
                    Surface = Color.FromRgb(255, 255, 255),
                    OnSurface = Color.FromRgb(23, 23, 23),
                    SurfaceVariant = Color.FromRgb(254, 252, 232),
                    OnSurfaceVariant = Color.FromRgb(82, 82, 82),
                    
                    Background = Color.FromRgb(254, 252, 232),
                    OnBackground = Color.FromRgb(38, 38, 38),
                    
                    Outline = Color.FromRgb(229, 229, 229),
                    OutlineVariant = Color.FromRgb(212, 212, 212),
                    
                    Error = Color.FromRgb(239, 68, 68),
                    Success = Color.FromRgb(34, 197, 94),
                    Warning = Color.FromRgb(251, 146, 60),
                    Info = Color.FromRgb(59, 130, 246),
                    
                    TextPrimary = Color.FromRgb(23, 23, 23),
                    TextSecondary = Color.FromRgb(115, 115, 115)
                },

                CircadianPeriod.Noon => new ColorPalette
                {
                    Primary = Color.FromRgb(14, 165, 233),
                    OnPrimary = Color.FromRgb(255, 255, 255),
                    PrimaryContainer = Color.FromRgb(224, 242, 254),
                    OnPrimaryContainer = Color.FromRgb(7, 89, 133),
                    
                    Secondary = Color.FromRgb(6, 182, 212),
                    OnSecondary = Color.FromRgb(255, 255, 255),
                    SecondaryContainer = Color.FromRgb(207, 250, 254),
                    
                    Surface = Color.FromRgb(255, 255, 255),
                    OnSurface = Color.FromRgb(15, 23, 42),
                    SurfaceVariant = Color.FromRgb(240, 249, 255),
                    OnSurfaceVariant = Color.FromRgb(71, 85, 105),
                    
                    Background = Color.FromRgb(248, 250, 252),
                    OnBackground = Color.FromRgb(30, 41, 59),
                    
                    Outline = Color.FromRgb(226, 232, 240),
                    OutlineVariant = Color.FromRgb(203, 213, 225),
                    
                    Error = Color.FromRgb(239, 68, 68),
                    Success = Color.FromRgb(16, 185, 129),
                    Warning = Color.FromRgb(245, 158, 11),
                    Info = Color.FromRgb(59, 130, 246),
                    
                    TextPrimary = Color.FromRgb(15, 23, 42),
                    TextSecondary = Color.FromRgb(100, 116, 139)
                },

                CircadianPeriod.Afternoon => new ColorPalette
                {
                    Primary = Color.FromRgb(6, 182, 212),
                    OnPrimary = Color.FromRgb(255, 255, 255),
                    PrimaryContainer = Color.FromRgb(207, 250, 254),
                    OnPrimaryContainer = Color.FromRgb(8, 145, 178),
                    
                    Secondary = Color.FromRgb(14, 165, 233),
                    OnSecondary = Color.FromRgb(255, 255, 255),
                    SecondaryContainer = Color.FromRgb(224, 242, 254),
                    
                    Surface = Color.FromRgb(255, 255, 255),
                    OnSurface = Color.FromRgb(15, 23, 42),
                    SurfaceVariant = Color.FromRgb(241, 245, 249),
                    OnSurfaceVariant = Color.FromRgb(71, 85, 105),
                    
                    Background = Color.FromRgb(248, 250, 252),
                    OnBackground = Color.FromRgb(30, 41, 59),
                    
                    Outline = Color.FromRgb(226, 232, 240),
                    OutlineVariant = Color.FromRgb(203, 213, 225),
                    
                    Error = Color.FromRgb(239, 68, 68),
                    Success = Color.FromRgb(34, 197, 94),
                    Warning = Color.FromRgb(251, 146, 60),
                    Info = Color.FromRgb(59, 130, 246),
                    
                    TextPrimary = Color.FromRgb(15, 23, 42),
                    TextSecondary = Color.FromRgb(100, 116, 139)
                },

                CircadianPeriod.LateAfternoon => new ColorPalette
                {
                    Primary = Color.FromRgb(59, 130, 246),
                    OnPrimary = Color.FromRgb(255, 255, 255),
                    PrimaryContainer = Color.FromRgb(219, 234, 254),
                    OnPrimaryContainer = Color.FromRgb(30, 64, 175),
                    
                    Secondary = Color.FromRgb(168, 85, 247),
                    OnSecondary = Color.FromRgb(255, 255, 255),
                    SecondaryContainer = Color.FromRgb(237, 233, 254),
                    
                    Surface = Color.FromRgb(255, 255, 255),
                    OnSurface = Color.FromRgb(15, 23, 42),
                    SurfaceVariant = Color.FromRgb(248, 250, 252),
                    OnSurfaceVariant = Color.FromRgb(71, 85, 105),
                    
                    Background = Color.FromRgb(241, 245, 249),
                    OnBackground = Color.FromRgb(30, 41, 59),
                    
                    Outline = Color.FromRgb(226, 232, 240),
                    OutlineVariant = Color.FromRgb(203, 213, 225),
                    
                    Error = Color.FromRgb(239, 68, 68),
                    Success = Color.FromRgb(34, 197, 94),
                    Warning = Color.FromRgb(251, 146, 60),
                    Info = Color.FromRgb(96, 165, 250),
                    
                    TextPrimary = Color.FromRgb(15, 23, 42),
                    TextSecondary = Color.FromRgb(100, 116, 139)
                },

                CircadianPeriod.Dusk => new ColorPalette
                {
                    Primary = Color.FromRgb(251, 146, 60),
                    OnPrimary = Color.FromRgb(67, 20, 7),
                    PrimaryContainer = Color.FromRgb(255, 237, 213),
                    OnPrimaryContainer = Color.FromRgb(154, 52, 18),
                    
                    Secondary = Color.FromRgb(236, 72, 153),
                    OnSecondary = Color.FromRgb(80, 7, 36),
                    SecondaryContainer = Color.FromRgb(252, 231, 243),
                    
                    Surface = Color.FromRgb(254, 252, 232),
                    OnSurface = Color.FromRgb(28, 25, 23),
                    SurfaceVariant = Color.FromRgb(255, 248, 225),
                    OnSurfaceVariant = Color.FromRgb(68, 64, 60),
                    
                    Background = Color.FromRgb(255, 251, 235),
                    OnBackground = Color.FromRgb(41, 37, 36),
                    
                    Outline = Color.FromRgb(231, 229, 228),
                    OutlineVariant = Color.FromRgb(214, 211, 209),
                    
                    Error = Color.FromRgb(220, 38, 38),
                    Success = Color.FromRgb(34, 197, 94),
                    Warning = Color.FromRgb(245, 158, 11),
                    Info = Color.FromRgb(59, 130, 246),
                    
                    TextPrimary = Color.FromRgb(28, 25, 23),
                    TextSecondary = Color.FromRgb(87, 83, 78)
                },

                CircadianPeriod.Evening => new ColorPalette
                {
                    Primary = Color.FromRgb(168, 85, 247),
                    OnPrimary = Color.FromRgb(255, 255, 255),
                    PrimaryContainer = Color.FromRgb(107, 33, 168),
                    OnPrimaryContainer = Color.FromRgb(237, 233, 254),
                    
                    Secondary = Color.FromRgb(139, 92, 246),
                    OnSecondary = Color.FromRgb(255, 255, 255),
                    SecondaryContainer = Color.FromRgb(88, 28, 135),
                    
                    Surface = Color.FromRgb(41, 37, 36),
                    OnSurface = Color.FromRgb(250, 250, 249),
                    SurfaceVariant = Color.FromRgb(57, 52, 52),
                    OnSurfaceVariant = Color.FromRgb(231, 229, 228),
                    
                    Background = Color.FromRgb(28, 25, 23),
                    OnBackground = Color.FromRgb(254, 252, 232),
                    
                    Outline = Color.FromRgb(68, 64, 60),
                    OutlineVariant = Color.FromRgb(87, 83, 78),
                    
                    Error = Color.FromRgb(248, 113, 113),
                    Success = Color.FromRgb(74, 222, 128),
                    Warning = Color.FromRgb(251, 191, 36),
                    Info = Color.FromRgb(147, 197, 253),
                    
                    TextPrimary = Color.FromRgb(250, 250, 249),
                    TextSecondary = Color.FromRgb(214, 211, 209)
                },

                CircadianPeriod.Night => new ColorPalette
                {
                    Primary = Color.FromRgb(147, 51, 234),
                    OnPrimary = Color.FromRgb(250, 245, 255),
                    PrimaryContainer = Color.FromRgb(88, 28, 135),
                    OnPrimaryContainer = Color.FromRgb(233, 213, 255),
                    
                    Secondary = Color.FromRgb(96, 165, 250),
                    OnSecondary = Color.FromRgb(30, 58, 138),
                    SecondaryContainer = Color.FromRgb(37, 99, 235),
                    
                    Surface = Color.FromRgb(24, 24, 27),
                    OnSurface = Color.FromRgb(244, 244, 245),
                    SurfaceVariant = Color.FromRgb(39, 39, 42),
                    OnSurfaceVariant = Color.FromRgb(228, 228, 231),
                    
                    Background = Color.FromRgb(9, 9, 11),
                    OnBackground = Color.FromRgb(250, 250, 250),
                    
                    Outline = Color.FromRgb(63, 63, 70),
                    OutlineVariant = Color.FromRgb(82, 82, 91),
                    
                    Error = Color.FromRgb(248, 113, 113),
                    Success = Color.FromRgb(52, 211, 153),
                    Warning = Color.FromRgb(251, 191, 36),
                    Info = Color.FromRgb(96, 165, 250),
                    
                    TextPrimary = Color.FromRgb(244, 244, 245),
                    TextSecondary = Color.FromRgb(161, 161, 170)
                },

                _ => GetPaletteForPeriod(CircadianPeriod.Noon)
            };
        }

        private void ApplyPalette(IResourceDictionary resources, ColorPalette palette)
        {
            ApplyBrush(resources, "PrimaryBrush", palette.Primary);
            ApplyBrush(resources, "OnPrimaryBrush", palette.OnPrimary);
            ApplyBrush(resources, "PrimaryContainerBrush", palette.PrimaryContainer);
            ApplyBrush(resources, "OnPrimaryContainerBrush", palette.OnPrimaryContainer);
            
            ApplyBrush(resources, "SecondaryBrush", palette.Secondary);
            ApplyBrush(resources, "OnSecondaryBrush", palette.OnSecondary);
            ApplyBrush(resources, "SecondaryContainerBrush", palette.SecondaryContainer);
            
            ApplyBrush(resources, "SurfaceBrush", palette.Surface);
            ApplyBrush(resources, "OnSurfaceBrush", palette.OnSurface);
            ApplyBrush(resources, "SurfaceVariantBrush", palette.SurfaceVariant);
            ApplyBrush(resources, "OnSurfaceVariantBrush", palette.OnSurfaceVariant);
            
            ApplyBrush(resources, "BackgroundBrush", palette.Background);
            ApplyBrush(resources, "OnBackgroundBrush", palette.OnBackground);
            
            ApplyBrush(resources, "OutlineBrush", palette.Outline);
            ApplyBrush(resources, "OutlineVariantBrush", palette.OutlineVariant);
            
            ApplyBrush(resources, "ErrorBrush", palette.Error);
            ApplyBrush(resources, "SuccessBrush", palette.Success);
            ApplyBrush(resources, "WarningBrush", palette.Warning);
            ApplyBrush(resources, "InfoBrush", palette.Info);
            
            ApplyBrush(resources, "TextPrimaryBrush", palette.TextPrimary);
            ApplyBrush(resources, "TextSecondaryBrush", palette.TextSecondary);
        }

        private void ApplyBrush(IResourceDictionary resources, string key, Color color)
        {
            try
            {
                resources[key] = new SolidColorBrush(color);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Failed to apply brush '{key}': {ex.Message}");
                // Fallback to default color
                resources[key] = new SolidColorBrush(Colors.Gray);
            }
        }

        private class ColorPalette
        {
            public Color Primary { get; set; }
            public Color OnPrimary { get; set; }
            public Color PrimaryContainer { get; set; }
            public Color OnPrimaryContainer { get; set; }
            
            public Color Secondary { get; set; }
            public Color OnSecondary { get; set; }
            public Color SecondaryContainer { get; set; }
            
            public Color Surface { get; set; }
            public Color OnSurface { get; set; }
            public Color SurfaceVariant { get; set; }
            public Color OnSurfaceVariant { get; set; }
            
            public Color Background { get; set; }
            public Color OnBackground { get; set; }
            
            public Color Outline { get; set; }
            public Color OutlineVariant { get; set; }
            
            public Color Error { get; set; }
            public Color Success { get; set; }
            public Color Warning { get; set; }
            public Color Info { get; set; }
            
            public Color TextPrimary { get; set; }
            public Color TextSecondary { get; set; }
        }
    }
}