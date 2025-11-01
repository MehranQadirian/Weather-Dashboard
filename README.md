# Weather Dashboard

A modern cross-platform weather application built with Avalonia UI and .NET 9, featuring real-time data, dynamic animations, and adaptive themes.

![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-blue)
![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![Avalonia](https://img.shields.io/badge/Avalonia-11.3.7-orange)

## Features

- **UI Design**: Material Design 3 with glassmorphism, smooth animations, circadian themes (12 periods), interactive charts, and responsive layout.
- **Weather Data**: Real-time from OpenWeatherMap, 7-day forecast, hourly trends, multi-city tracking, advanced search (e.g., `rainy:london`).
- **Animations**: Realistic effects for clouds, rain, snow, lightning, fog, sun, and day/night cycles based on sunrise/sunset.
- **Data Management**: Local storage for favorites, caching, persistent settings, auto-refresh.

## Screenshots

### Main Dashboard
![Main Dashboard](https://github.com/user-attachments/assets/1172bf2d-5855-4c1c-a964-bb7046d6132a)

## Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Installation
1. Clone the repository:
   ```
   git clone https://github.com/mehranqadirian/Weather-Dashboard.git
   cd Weather-Dashboard
   ```
2. Restore dependencies:
   ```
   dotnet restore
   ```
3. Configure API Key: Replace `YOUR_API_KEY` in `Services/WeatherApiService.cs` with your [OpenWeatherMap key](https://openweathermap.org/api).
4. Build and run:
   ```
   dotnet build
   dotnet run --project Weather.Dashboard.Avalonia
   ```

## Pre-built Releases

Download the latest version from [Releases](https://github.com/mehranqadirian/Weather-Dashboard/releases/latest).

### Windows
- Portable: [win-x64.zip](https://github.com/mehranqadirian/Weather-Dashboard/releases/latest/download/win-x64.zip)

### Linux
- Portable: [linux-x64.zip](https://github.com/mehranqadirian/Weather-Dashboard/releases/latest/download/linux-x64.zip)

### macOS
- Intel: [mac-x64.zip](https://github.com/mehranqadirian/Weather-Dashboard/releases/latest/download/mac-x64.zip)
- Apple Silicon: [mac-arm64.zip](https://github.com/mehranqadirian/Weather-Dashboard/releases/latest/download/mac-arm64.zip)

## Advanced Search
Search bar supports filters:
- City: `london`
- Conditions: `rainy:tokyo`, `snowy:moscow`, `sunny:california` (also cloudy, storm, partly, foggy).

## Architecture

### Project Structure
```
Weather.Dashboard.Avalonia/
├── Controls/              # Custom UI controls
│   ├── AnimatedBackgroundControl.axaml
│   ├── CityWeatherCard.axaml
│   ├── DetailView.axaml
│   └── SimpleLineChart.axaml
├── Services/             # Business logic
│   ├── WeatherApiService.cs
│   ├── CacheService.cs
│   ├── CircadianThemeService.cs
│   └── AnimationService.cs
├── ViewModels/           # MVVM ViewModels
│   ├── MainViewModel.cs
│   └── CityWeatherViewModel.cs
├── Models/               # Data models
│   ├── CurrentWeather.cs
│   ├── ForecastItem.cs
│   └── AnimationState.cs
└── Styles/               # UI styling
    ├── CommonStyles.axaml
    └── Colors.axaml
```

### Technologies
- Avalonia UI 11.3.7
- .NET 9.0
- Polly for resilience
- System.Text.Json
- OpenWeatherMap API
- Material Design 3

## Customization
- Edit `AnimationState.cs` for particle/speed adjustments.
- Extend `WeatherCondition` enum in `Models/WeatherCondition.cs` for new conditions.
- Modify `CircadianThemeService.cs` for theme colors.

## Acknowledgments
- Weather: [OpenWeatherMap](https://openweathermap.org/)
- Icons: Material Design
- Powered by [Lumora Flow](https://lumora-flow.pages.dev)
- Built with [Avalonia UI](https://avaloniaui.net/)

## Contact
- Developer: Mehran Qadirian
- Email: mehran.qadirian@example.com (update as needed)
- Project: https://github.com/mehranqadirian/Weather-Dashboard

## Known Issues
- AppImage needs testing on Linux.
- Animations may lag on older hardware.
- API limits: 60 calls/minute (free tier).

## Roadmap
- Air quality display
- Alerts/notifications
- Historical charts
- Desktop widgets
- Multi-language
- Theme override
- CSV export

---

<div align="center">
  <p>Made with ❤️ using Avalonia UI</p>
  <p>⭐ Star this repo if you find it useful!</p>
</div>