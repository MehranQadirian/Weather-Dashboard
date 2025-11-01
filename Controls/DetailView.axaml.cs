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
    }
}