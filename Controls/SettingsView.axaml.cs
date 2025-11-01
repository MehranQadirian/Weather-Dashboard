using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Weather.Dashboard.Avalonia.ViewModels;

namespace Weather.Dashboard.Avalonia.Controls
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void CloseSettings_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.CloseCommand.Execute(null);
            }
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.ApplyCommand.Execute(null);
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.ResetCommand.Execute(null);
            }
        }
    }
}