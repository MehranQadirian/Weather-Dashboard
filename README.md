# 🌤️ Weather Dashboard

A beautiful, modern weather dashboard application built with Avalonia UI and .NET 9, featuring stunning animations, circadian theme adaptation, and real-time weather data.

![Weather Dashboard](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20macOS-blue)
![.NET Version](https://img.shields.io/badge/.NET-9.0-purple)
![Avalonia](https://img.shields.io/badge/Avalonia-11.3.7-orange)
![License](https://img.shields.io/badge/License-MIT-green)

## ✨ Features

### 🎨 Beautiful UI Design
- **Material Design 3** inspired interface with glassmorphism effects
- **Smooth animations** for weather conditions (rain, snow, clouds, fog)
- **Circadian rhythm theming** - automatically adapts colors based on time of day (12 different periods)
- **Interactive charts** showing 24-hour temperature trends
- **Responsive design** that works seamlessly across all screen sizes

### 🌍 Weather Features
- **Real-time weather data** from OpenWeatherMap API
- **7-day forecast** with detailed information
- **Hourly temperature charts** with smooth interpolation
- **Multiple cities tracking** - add and manage favorite cities
- **Advanced search** with condition filters (e.g., `rainy:london`, `snowy:moscow`)
- **Weather condition filtering** - filter cities by weather type

### 🎭 Dynamic Animations
- **Realistic cloud formations** with 3 different cloud types (Wispy, Puffy, Cumulus)
- **Rain animation** with wind effects
- **Snow particles** with realistic falling patterns
- **Lightning effects** during storms
- **Fog layers** with depth simulation
- **Sun particles** for clear weather
- **Day/night cycle** based on actual sunrise/sunset times

### 🌈 Circadian Theme System
Automatic theme adaptation based on 12 time periods:
- 🌙 **Deep Night** (00:00-02:00) - Deep purples and blues
- 🌌 **Late Night** (02:00-04:00) - Dark violet tones
- 🌄 **Pre-Dawn** (04:00-06:00) - Indigo transitions
- 🌅 **Dawn** (06:00-08:00) - Orange and pink sunrise
- ☀️ **Early Morning** (08:00-10:00) - Warm amber colors
- 🌤️ **Late Morning** (10:00-12:00) - Bright yellows
- ☀️ **Noon** (12:00-14:00) - Vibrant sky blues
- 🌤️ **Afternoon** (14:00-16:00) - Clear cyan tones
- 🌆 **Late Afternoon** (16:00-18:00) - Blue to purple transition
- 🌇 **Dusk** (18:00-20:00) - Orange sunset glow
- 🌃 **Evening** (20:00-22:00) - Deep purple evening
- 🌙 **Night** (22:00-00:00) - Dark blue night sky

### 💾 Data Management
- **Local storage** for favorite cities
- **Smart caching** system to reduce API calls
- **Persistent configuration** for user preferences
- **Automatic data refresh** with configurable intervals

## 🖼️ Screenshots

### Main Dashboard
![Main Dashboard](<img width="1918" height="1020" alt="image" src="https://github.com/user-attachments/assets/1172bf2d-5855-4c1c-a964-bb7046d6132a" />)

## 🚀 Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Windows 10/11, Linux, or macOS

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/yourusername/weather-dashboard.git
cd weather-dashboard
```

2. **Restore dependencies**
```bash
dotnet restore
```

3. **Configure API Key**
   - Open `Services/WeatherApiService.cs`
   - Replace `YOUR_API_KEY` with your OpenWeatherMap API key
   - Get a free API key at: https://openweathermap.org/api

4. **Build and run**
```bash
dotnet build
dotnet run --project Weather.Dashboard.Avalonia
```

## 📦 Download Pre-built Releases

Get the latest version for your platform:

### Windows
- **Installer**: `WeatherDashboard-Setup-Windows-x64.exe`
- **Portable**: `WeatherDashboard-Windows-x64.zip`

### Linux
- **DEB Package**: `WeatherDashboard-linux-x64.deb`
- **Portable**: `WeatherDashboard-Linux-x64.zip`

### macOS
- **DMG**: `WeatherDashboard-macOS.dmg`
- **Intel**: `WeatherDashboard-macOS-x64.zip`
- **Apple Silicon**: `WeatherDashboard-macOS-arm64.zip`

Download from [Releases](https://github.com/mehranqadirian/Weather-Dashboard/releases)

## 🎯 Advanced Search

The search bar supports advanced filtering:

```
# Search by city name
london

# Find rainy cities
rainy:tokyo

# Find snowy locations
snowy:moscow

# Find sunny places
sunny:california

# Other conditions: cloudy, storm, partly, foggy
```

## 🏗️ Architecture

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

### Technologies Used
- **Avalonia UI 11.3.7** - Cross-platform UI framework
- **.NET 9.0** - Runtime framework
- **Polly** - Resilience and transient-fault-handling
- **System.Text.Json** - JSON serialization
- **OpenWeatherMap API** - Weather data provider
- **Material Design 3** - Design system

## 🎨 Customization

### Change Animation Intensity
Edit `AnimationState.cs` to modify particle counts and animation speeds.

### Add Custom Weather Conditions
Extend `WeatherCondition` enum in `Models/WeatherCondition.cs` and add corresponding animations.

### Modify Theme Colors
Edit `Services/CircadianThemeService.cs` to customize the color palettes for each time period.

## 🙏 Acknowledgments

- Weather data provided by [OpenWeatherMap](https://openweathermap.org/)
- Icons from Material Design
- Powered by [Lumora Flow](https://lumora-flow.pages.dev)
- Built with [Avalonia UI](https://avaloniaui.net/)

## 📧 Contact

- **Developer**: Your Name
- **Email**: your.email@example.com
- **Project Link**: https://github.com/yourusername/weather-dashboard

## 🐛 Known Issues

- AppImage support for Linux requires additional testing
- Some animations may be intensive on older hardware
- API rate limits apply (60 calls/minute on free tier)

## 🔮 Roadmap

- [ ] Air quality index display
- [ ] Weather alerts and notifications
- [ ] Historical weather data charts
- [ ] Weather widgets for desktop
- [ ] Multi-language support
- [ ] Dark/Light theme manual override
- [ ] Export weather data to CSV

---

<div align="center">
  <p>Made with ❤️ using Avalonia UI</p>
  <p>⭐ Star this repo if you find it useful!</p>
</div>
