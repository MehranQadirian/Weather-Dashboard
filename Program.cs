using System;
using Avalonia;

namespace Weather.Dashboard.Avalonia
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🚀 Application starting...");
        
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);  
            
                System.Diagnostics.Debug.WriteLine("✅ Application closed normally");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Fatal error: {ex.Message}");
                throw;
            }
        }

        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
        }
    }
}