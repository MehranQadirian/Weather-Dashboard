using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Weather.Dashboard.Avalonia.ViewModels;

namespace Weather.Dashboard.Avalonia.Controls
{
    public partial class CityWeatherCard : UserControl
    {
        public CityWeatherCard()
        {
            InitializeComponent();
        }

        private void CardBorder_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is CityWeatherViewModel cityViewModel)
            {
                var mainWindow = TopLevel.GetTopLevel(this) as MainWindow;
                if (mainWindow?.DataContext is MainViewModel mainViewModel)
                {
                    mainViewModel.SelectedCity = cityViewModel;
                }
            }
        }
    }
}