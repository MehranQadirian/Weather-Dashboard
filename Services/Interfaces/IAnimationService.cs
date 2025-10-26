using Weather.Dashboard.Avalonia.Models;

namespace Weather.Dashboard.Avalonia.Services.Interfaces
{
    public interface IAnimationService
    {
        AnimationState GetAnimationState(CurrentWeather weather);
        void UpdateIntensity(AnimationState state, double intensity);
    }
}