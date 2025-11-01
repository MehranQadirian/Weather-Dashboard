using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Weather.Dashboard.Avalonia.Controls;
using Weather.Dashboard.Avalonia.Services;
using Weather.Dashboard.Avalonia.ViewModels;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Threading;

namespace Weather.Dashboard.Avalonia
{
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;
        private SettingsViewModel? _settingsViewModel;
        private DispatcherTimer? _fadeTimer;
        private bool _isFadingIn;
        private const double FadeStep = 0.05;
        private const int FadeInterval = 15;
        public MainWindow()
        {
            try
            {
                Debug.WriteLine("🔧 MainWindow constructor started...");
                InitializeComponent();
                SetDynamicWindowSize();
                InitializeViewModel();
                
                Debug.WriteLine("✅ MainWindow initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ MainWindow constructor error: {ex.Message}");
                Debug.WriteLine($"   Stack: {ex.StackTrace}");
                
                ShowErrorDialog(ex);
            }
            _fadeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(FadeInterval)
            };
            _fadeTimer.Tick += FadeTimer_Tick;
        }
        private void LumoraBorder_PointerEntered(object? sender, PointerEventArgs e)
        {
            _isFadingIn = true;
            _fadeTimer?.Start();
        }

        private void LumoraBorder_PointerExited(object? sender, PointerEventArgs e)
        {
            _isFadingIn = false;
            _fadeTimer?.Start();
        }

        private void FadeTimer_Tick(object? sender, EventArgs e)
        {
            if (_isFadingIn)
            {
                double newOpacity = Math.Min(1.0, LeftBracket.Opacity + FadeStep);
                LeftBracket.Opacity = newOpacity;
                RightBracket.Opacity = newOpacity;
                
                if (newOpacity >= 1.0)
                {
                    _fadeTimer?.Stop();
                }
            }
            else
            {
                double newOpacity = Math.Max(0.0, LeftBracket.Opacity - FadeStep);
                LeftBracket.Opacity = newOpacity;
                RightBracket.Opacity = newOpacity;
                
                if (newOpacity <= 0.0)
                {
                    _fadeTimer?.Stop();
                }
            }
        }
        private void SetDynamicWindowSize()
        {
            try
            {
                var screen = Screens.Primary;
                if (screen == null)
                {
                    Debug.WriteLine("⚠️ Primary screen not found, using defaults");
                    return;
                }

                var workingArea = screen.WorkingArea;
                Debug.WriteLine($"📊 Screen Info:");
                Debug.WriteLine($"   Working Area: {workingArea.Width} x {workingArea.Height}");
                Debug.WriteLine($"   Position: X={workingArea.X}, Y={workingArea.Y}");

                const double reductionPercentage = 0.72; 
                
                double newWidth = workingArea.Width * reductionPercentage;
                double newHeight = workingArea.Height * reductionPercentage;

                newWidth = Math.Max(newWidth, 900);
                newHeight = Math.Max(newHeight, 600);

                Width = newWidth;
                Height = newHeight;

                Debug.WriteLine($"📐 Dynamic Window Size Applied:");
                Debug.WriteLine($"   New Size: {newWidth:F0} x {newHeight:F0}");
                Debug.WriteLine($"   Reduction: 20% from screen resolution");

                SaveWindowSizeToConfig(newWidth, newHeight);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ SetDynamicWindowSize failed: {ex.Message}");
            }
        }
        private void SaveWindowSizeToConfig(double width, double height)
        {
            try
            {
                var app = (App)Application.Current!;
                var configService = app.ServiceProvider?.GetService<IConfigurationService>();
                
                if (configService != null)
                {
                    configService.Configuration.WindowWidth = width;
                    configService.Configuration.WindowHeight = height;
                    _ = configService.SaveAsync();
                    
                    Debug.WriteLine($"💾 Window size saved to config");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Failed to save window config: {ex.Message}");
            }
        }
        private void InitializeViewModel()
        {
            try
            {
                var app = (App)Application.Current!;
                
                if (app.ServiceProvider == null)
                {
                    throw new InvalidOperationException("ServiceProvider is null! DI container not initialized.");
                }

                _viewModel = app.ServiceProvider.GetRequiredService<MainViewModel>();
                
                if (_viewModel == null)
                {
                    throw new InvalidOperationException("MainViewModel could not be resolved from DI container.");
                }

                DataContext = _viewModel;
                
                Debug.WriteLine("✅ MainViewModel successfully set as DataContext");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ InitializeViewModel error: {ex.Message}");
                throw;
            }
        }

        private void ShowErrorDialog(Exception ex)
        {
            var errorWindow = new Window
            {
                Title = "Initialization Error",
                Width = 500,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = new StackPanel
                {
                    Margin = new Thickness(20),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Failed to initialize Weather Dashboard",
                            FontSize = 18,
                            FontWeight = FontWeight.Bold,
                            Margin = new Thickness(0, 0, 0, 20)
                        },
                        new TextBlock
                        {
                            Text = $"Error: {ex.Message}",
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 0, 0, 10)
                        },
                        new TextBlock
                        {
                            Text = "Check Debug Output for more details.",
                            FontStyle = FontStyle.Italic,
                            Foreground = Brushes.Gray
                        }
                    }
                }
            };

            errorWindow.Show();
        }
        private async void RefreshButton_Click(object? sender, RoutedEventArgs e)
        {
            if(sender is Button button)
            {
                await AnimateRefreshButton(button);
                if(DataContext is MainViewModel mainViewModel)
                {
                    await mainViewModel.RefreshAllAsync();
                }
            }
        }

        private async Task AnimateRefreshButton(Button button)
        {
            var rotateTransform = new RotateTransform();
            button.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
            button.RenderTransform = rotateTransform;

            const int steps = 72;
            const int durationMs = 200;
            var delay = durationMs / steps;

            for(int i = 0; i <= steps; i++)
            {
                double progress = (double)i / steps;
                double angle = progress * 180;
                rotateTransform.Angle = angle;
                await Task.Delay(delay);
            }
        }
        private async Task AnimateSettingsButton(Button button)
        {
            var rotateTransform = new RotateTransform();
            button.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
            button.RenderTransform = rotateTransform;

            const int steps = 36;
            const int durationMs = 100;
            var delay = durationMs / steps;

            for(int i = 0; i <= steps; i++)
            {
                double progress = (double)i / steps;
                double angle = progress * 90;
                rotateTransform.Angle = angle;
                await Task.Delay(delay);
            }
        }
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            Debug.WriteLine("✅ MainWindow attached to visual tree");
        }

        private void LumoraFlow_Click(object? sender, PointerPressedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://lumora-flow.pages.dev",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening Lumora Flow: " + ex.Message);
            }
        }

        private async void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            if(sender  is Button button)
                await AnimateSettingsButton(button);
            if (_viewModel == null)
            {
                Debug.WriteLine("⚠️ Cannot open settings - ViewModel is null");
                return;
            }

            var app = (App)Application.Current!;
            var settingsService = app.ServiceProvider!.GetRequiredService<ISettingsService>();

            _settingsViewModel = new SettingsViewModel(settingsService, _viewModel);

            var settingsOverlay = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
                ClipToBounds = true,
                ZIndex = 1000
            };

            var settingsContainer = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                CornerRadius = new CornerRadius(0),
                Margin = new Thickness(0),
                ClipToBounds = true,
                Child = new SettingsView
                {
                    DataContext = _settingsViewModel
                }
            };

            settingsOverlay.Child = settingsContainer;

            var mainGrid = this.Get<Grid>("MainGrid");
            if (mainGrid != null)
            {
                mainGrid.Children.Add(settingsOverlay);
                Grid.SetRow(settingsOverlay, 0);
                Grid.SetRowSpan(settingsOverlay, 4);

                _settingsViewModel.SettingsClosed += () =>
                {
                    mainGrid.Children.Remove(settingsOverlay);
                };
            }
        }

        private void CityCard_Click(object sender, PointerPressedEventArgs e)
        {
            if (sender is Control control && control.DataContext is CityWeatherViewModel cityViewModel)
            {
                if (DataContext is MainViewModel mainViewModel)
                {
                    mainViewModel.SelectedCity = cityViewModel;
                }
            }
        }
    }
}