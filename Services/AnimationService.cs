using Weather.Dashboard.Avalonia.Models;
using Weather.Dashboard.Avalonia.Services.Interfaces;

namespace Weather.Dashboard.Avalonia.Services
{
    public class AnimationService : IAnimationService
    {
        public AnimationState GetAnimationState(CurrentWeather weather)
        {
            return AnimationState.FromWeather(weather);
        }

        public void UpdateIntensity(AnimationState state, double intensity)
        {
            state.Intensity = System.Math.Max(0, System.Math.Min(1, intensity));

            // Adjust particle count based on intensity
            switch (state.Condition)
            {
                case WeatherCondition.Rainy:
                    state.ParticleCount = (int)(150 * state.Intensity);
                    break;
                case WeatherCondition.Snowy:
                    state.ParticleCount = (int)(80 * state.Intensity);
                    break;
                case WeatherCondition.Storm:
                    state.ParticleCount = (int)(200 * state.Intensity);
                    break;
            }
        }
    }
}