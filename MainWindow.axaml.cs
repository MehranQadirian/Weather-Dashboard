using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Weather.Dashboard.Avalonia.ViewModels;

namespace Weather.Dashboard.Avalonia
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            var app = (App)Application.Current;
            _viewModel = app.ServiceProvider?.GetRequiredService<MainViewModel>();
            DataContext = _viewModel;
            System.Diagnostics.Debug.WriteLine("✅ MainWindow loaded with ViewModel");
        }
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.FindAncestorOfType<Border>() is Border border)
            {
                //border.BorderBrush = Application.Current.Resources["PrimaryBrush"] as IBrush;
                //border.BorderThickness = new Thickness(2);
            }
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
        
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.FindAncestorOfType<Border>() is Border border)
            {
                //border.BorderBrush = Application.Current.Resources["OutlineBrush"] as IBrush;
                //border.BorderThickness = new Thickness(1);
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