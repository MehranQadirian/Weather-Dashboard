using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Weather.Dashboard.Avalonia.ViewModels;
namespace Weather.Dashboard.Avalonia.Controls
{
    public partial class DetailView : UserControl
    {
        public DetailView()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is CityWeatherViewModel viewModel)
            {
                var mainWindow = (MainWindow)TopLevel.GetTopLevel(this);
                if (mainWindow?.DataContext is MainViewModel mainVM)
                {
                    mainVM.SelectCityCommand?.Execute(null);
                }
            }
        }

        private void IntensitySlider_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Value" && DataContext is CityWeatherViewModel viewModel)
            {
                var newIntensity = (double)e.NewValue;
                
                // 🔥 به‌روزرسانی تعداد ذرات (Particles)
                if (viewModel.AnimationState != null)
                {
                    switch (viewModel.AnimationState.Condition)
                    {
                        case Models.WeatherCondition.Rainy:
                            viewModel.AnimationState.ParticleCount = (int)(150 * newIntensity);
                            break;
                        case Models.WeatherCondition.Snowy:
                            viewModel.AnimationState.ParticleCount = (int)(80 * newIntensity);
                            break;
                        case Models.WeatherCondition.Storm:
                            viewModel.AnimationState.ParticleCount = (int)(200 * newIntensity);
                            break;
                        default:
                            viewModel.AnimationState.ParticleCount = 0;
                            break;
                    }

                    // 🎨 اجبار به رندر مجدد AnimatedBackgroundControl
                    viewModel.OnPropertyChanged(nameof(viewModel.AnimationState));
                    
                    System.Diagnostics.Debug.WriteLine(
                        $"🎚️ Intensity changed: {newIntensity:P0} → {viewModel.AnimationState.ParticleCount} particles");
                }
            }
        }
    }
}