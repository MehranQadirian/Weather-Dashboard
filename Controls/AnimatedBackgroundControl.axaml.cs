using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;
using Weather.Dashboard.Avalonia.Models;

namespace Weather.Dashboard.Avalonia.Controls
{
    public partial class AnimatedBackgroundControl : UserControl
    {
        private DispatcherTimer _animationTimer;
        private List<Cloud> _clouds;
        private List<Particle> _particles;
        private Random _random;
        private AnimationState _currentState;
        private Rectangle _backgroundRect;
        private LinearGradientBrush _backgroundGradient;
        private GradientStop _gradientStop1;
        private GradientStop _gradientStop2;
        
        private const int MaxClouds = 8;
        private const int MaxParticles = 350;
        private const int FrameRate = 60;
        private const double OptimizationThreshold = 0.02;

        public static readonly StyledProperty<AnimationState> AnimationStateProperty =
            AvaloniaProperty.Register<AnimatedBackgroundControl, AnimationState>(nameof(AnimationState));

        public AnimationState AnimationState
        {
            get => GetValue(AnimationStateProperty);
            set => SetValue(AnimationStateProperty, value);
        }

        public AnimatedBackgroundControl()
        {
            InitializeComponent();
            _clouds = new List<Cloud>();
            _particles = new List<Particle>();
            _random = new Random();
            _animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000.0 / FrameRate) };
            _animationTimer.Tick += AnimationTimer_Tick;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            _backgroundRect = this.FindControl<Rectangle>("BackgroundRect");
            if (_backgroundRect?.Fill is LinearGradientBrush brush)
            {
                _backgroundGradient = brush;
                if (_backgroundGradient.GradientStops.Count >= 2)
                {
                    _gradientStop1 = _backgroundGradient.GradientStops[0];
                    _gradientStop2 = _backgroundGradient.GradientStops[1];
                }
            }
            _animationTimer.Start();
            if (AnimationState != null)
                UpdateAnimation(AnimationState);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            _animationTimer.Stop();
            ClearAllAnimations();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == AnimationStateProperty && change.NewValue is AnimationState state)
            {
                UpdateAnimation(state);
                if (change.OldValue is AnimationState oldState && 
                    (oldState.Condition != state.Condition || oldState.IsNight != state.IsNight))
                {
                    InitializeParticles(state);
                }
            }
        }

        private bool IsNightBasedOnLocation(CurrentWeather weather)
        {
            if (weather?.Sunrise == null || weather?.Sunset == null)
                return IsNightTime();

            var now = DateTime.Now;
            var sunrise = weather.Sunrise.Value;
            var sunset = weather.Sunset.Value;

            return now.TimeOfDay < sunrise.TimeOfDay || now.TimeOfDay > sunset.TimeOfDay;
        }

        private bool IsNightTime()
        {
            var hour = DateTime.Now.Hour;
            return hour < 6 || hour >= 18;
        }

        private void UpdateAnimation(AnimationState state)
        {
            if (state == null) return;
            _currentState = state;

            var (color1, color2) = GetWeatherColors(state.Condition, state.IsNight);
            if (_gradientStop1 != null)
                AnimateGradientColor(_gradientStop1, color1, 1.5);
            if (_gradientStop2 != null)
                AnimateGradientColor(_gradientStop2, color2, 1.5);

            InitializeParticles(state);
        }

        private (Color top, Color bottom) GetWeatherColors(WeatherCondition condition, bool isNight)
        {
            return condition switch
            {
                WeatherCondition.Sunny => isNight
                    ? (Color.FromRgb(30, 41, 59), Color.FromRgb(15, 23, 42))
                    : (Color.FromRgb(59, 130, 246), Color.FromRgb(37, 99, 235)),
                WeatherCondition.PartlyCloudy => isNight
                    ? (Color.FromRgb(51, 65, 85), Color.FromRgb(30, 41, 59))
                    : (Color.FromRgb(96, 165, 250), Color.FromRgb(59, 130, 246)),
                WeatherCondition.Cloudy => isNight
                    ? (Color.FromRgb(71, 85, 105), Color.FromRgb(51, 65, 85))
                    : (Color.FromRgb(148, 163, 184), Color.FromRgb(100, 116, 139)),
                WeatherCondition.Rainy => isNight
                    ? (Color.FromRgb(30, 58, 138), Color.FromRgb(17, 24, 39))
                    : (Color.FromRgb(37, 99, 235), Color.FromRgb(29, 78, 216)),
                WeatherCondition.Storm => isNight
                    ? (Color.FromRgb(55, 48, 163), Color.FromRgb(17, 24, 39))
                    : (Color.FromRgb(109, 40, 217), Color.FromRgb(88, 28, 135)),
                WeatherCondition.Snowy => isNight
                    ? (Color.FromRgb(71, 85, 105), Color.FromRgb(51, 65, 85))
                    : (Color.FromRgb(186, 230, 253), Color.FromRgb(125, 211, 252)),
                WeatherCondition.Foggy => isNight
                    ? (Color.FromRgb(82, 82, 91), Color.FromRgb(63, 63, 70))
                    : (Color.FromRgb(161, 161, 170), Color.FromRgb(113, 113, 122)),
                _ => isNight
                    ? (Color.FromRgb(30, 41, 59), Color.FromRgb(15, 23, 42))
                    : (Color.FromRgb(96, 165, 250), Color.FromRgb(59, 130, 246))
            };
        }

        private void AnimateGradientColor(GradientStop stop, Color targetColor, double durationSeconds)
        {
            if (stop == null) return;
            var startColor = stop.Color;
            var startTime = DateTime.Now;
            var duration = TimeSpan.FromSeconds(durationSeconds);
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };

            timer.Tick += (s, e) =>
            {
                var elapsed = DateTime.Now - startTime;
                var progress = Math.Min(1.0, elapsed.TotalSeconds / duration.TotalSeconds);
                var easedProgress = EaseInOutQuad(progress);

                var r = (byte)(startColor.R + (targetColor.R - startColor.R) * easedProgress);
                var g = (byte)(startColor.G + (targetColor.G - startColor.G) * easedProgress);
                var b = (byte)(startColor.B + (targetColor.B - startColor.B) * easedProgress);

                stop.Color = Color.FromRgb(r, g, b);

                if (progress >= 1.0)
                    timer.Stop();
            };
            timer.Start();
        }

        private double EaseInOutQuad(double t) => t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2;

        private void InitializeParticles(AnimationState state)
        {
            ClearAllAnimations();

            switch (state.Condition)
            {
                case WeatherCondition.Sunny:
                    CreateSunParticles(state);
                    break;
                case WeatherCondition.PartlyCloudy:
                    CreateRealisticClouds(state, 3);
                    CreateSunParticles(state);
                    break;
                case WeatherCondition.Cloudy:
                    CreateRealisticClouds(state, 5);
                    break;
                case WeatherCondition.Rainy:
                    CreateRealisticClouds(state, 4);
                    CreateRainParticles(state);
                    break;
                case WeatherCondition.Storm:
                    CreateRealisticClouds(state, 6);
                    CreateRainParticles(state);
                    CreateLightningFlashes(state);
                    break;
                case WeatherCondition.Snowy:
                    CreateRealisticClouds(state, 4);
                    CreateSnowParticles(state);
                    break;
                case WeatherCondition.Foggy:
                    CreateFogLayers(state);
                    break;
            }
        }

        private void CreateRealisticClouds(AnimationState state, int count)
        {
            if (Bounds.Width == 0 || Bounds.Height == 0) return;

            for (int i = 0; i < count && _clouds.Count < MaxClouds; i++)
            {
                var cloudSize = 0.5 + _random.NextDouble() * 1.2;
                var cloud = new Cloud
                {
                    X = _random.NextDouble() * Bounds.Width,
                    Y = _random.NextDouble() * (Bounds.Height * 0.5),
                    Speed = 8 + _random.NextDouble() * 25,
                    Scale = cloudSize,
                    Opacity = 0.35 + _random.NextDouble() * 0.5,
                    Type = (CloudType)(_random.Next(3)),
                    Depth = (int)(_random.NextDouble() * 3)
                };

                var cloudShape = CreateRealisticCloudShape(cloud, state.IsNight);
                AnimationCanvas.Children.Add(cloudShape);
                cloud.Element = cloudShape;
                _clouds.Add(cloud);
            }
        }

        private Control CreateRealisticCloudShape(Cloud cloud, bool isNight)
        {
            var canvas = new Canvas
            {
                Width = 140 * cloud.Scale,
                Height = 70 * cloud.Scale,
                Opacity = cloud.Opacity
            };

            var baseColor = isNight ? Color.FromArgb(100, 148, 163, 184) : Color.FromArgb(140, 255, 255, 255);
            var shadowColor = isNight ? Color.FromArgb(60, 100, 116, 139) : Color.FromArgb(80, 200, 200, 200);

            var bumps = cloud.Type switch
            {
                CloudType.Wispy => new[] {
                    (x: 15.0, y: 35.0, r: 18.0),
                    (x: 40.0, y: 25.0, r: 22.0),
                    (x: 70.0, y: 30.0, r: 20.0)
                },
                CloudType.Puffy => new[] {
                    (x: 20.0, y: 30.0, r: 22.0),
                    (x: 45.0, y: 20.0, r: 26.0),
                    (x: 75.0, y: 28.0, r: 23.0),
                    (x: 55.0, y: 38.0, r: 24.0)
                },
                _ => new[] {
                    (x: 25.0, y: 35.0, r: 20.0),
                    (x: 50.0, y: 22.0, r: 24.0),
                    (x: 80.0, y: 32.0, r: 21.0),
                    (x: 65.0, y: 42.0, r: 22.0)
                }
            };

            foreach (var (x, y, r) in bumps)
            {
                var shadowEllipse = new Ellipse
                {
                    Fill = new SolidColorBrush(shadowColor),
                    Width = r * 2 * cloud.Scale,
                    Height = r * 1.2 * cloud.Scale
                };
                Canvas.SetLeft(shadowEllipse, x * cloud.Scale - r * cloud.Scale);
                Canvas.SetTop(shadowEllipse, y * cloud.Scale - r * cloud.Scale + 2);
                canvas.Children.Add(shadowEllipse);

                var mainEllipse = new Ellipse
                {
                    Fill = new SolidColorBrush(baseColor),
                    Width = r * 2 * cloud.Scale,
                    Height = r * 2 * cloud.Scale
                };
                Canvas.SetLeft(mainEllipse, x * cloud.Scale - r * cloud.Scale);
                Canvas.SetTop(mainEllipse, y * cloud.Scale - r * cloud.Scale);
                canvas.Children.Add(mainEllipse);
            }

            Canvas.SetLeft(canvas, cloud.X);
            Canvas.SetTop(canvas, cloud.Y);
            return canvas;
        }

        private void CreateSunParticles(AnimationState state)
        {
            if (Bounds.Width == 0 || Bounds.Height == 0) return;

            int particleCount = 25;
            for (int i = 0; i < particleCount && _particles.Count < MaxParticles; i++)
            {
                var size = 2 + _random.NextDouble() * 4;
                var ellipse = new Ellipse
                {
                    Fill = new SolidColorBrush(Color.FromArgb(200, 255, 255, 150)),
                    Width = size,
                    Height = size
                };

                var particle = new Particle
                {
                    Element = ellipse,
                    X = _random.NextDouble() * Bounds.Width,
                    Y = _random.NextDouble() * Bounds.Height,
                    VelocityY = -0.4 + _random.NextDouble() * 0.3,
                    VelocityX = -0.15 + _random.NextDouble() * 0.3,
                    Rotation = _random.NextDouble() * 360,
                    LifeTime = 120 + _random.Next(80)
                };

                Canvas.SetLeft(ellipse, particle.X);
                Canvas.SetTop(ellipse, particle.Y);
                AnimationCanvas.Children.Add(ellipse);
                _particles.Add(particle);
            }
        }

        private void CreateRainParticles(AnimationState state)
        {
            if (Bounds.Width == 0 || Bounds.Height == 0) return;

            int particleCount = (int)(state.ParticleCount * state.Intensity);
            particleCount = Math.Min(particleCount, MaxParticles);

            for (int i = 0; i < particleCount && _particles.Count < MaxParticles; i++)
            {
                var length = 14 + _random.Next(10);
                var line = new Line
                {
                    Stroke = new SolidColorBrush(Color.FromArgb(200, 220, 240, 255)),
                    StrokeThickness = 1.8,
                    StrokeLineCap = PenLineCap.Round,
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, length)
                };

                var particle = new Particle
                {
                    Element = line,
                    X = _random.NextDouble() * Bounds.Width,
                    Y = _random.NextDouble() * Bounds.Height,
                    VelocityY = 12 + _random.NextDouble() * 10 * state.Intensity,
                    VelocityX = state.WindAngle / 8.0,
                    Opacity = 0.7 + _random.NextDouble() * 0.3
                };

                Canvas.SetLeft(line, particle.X);
                Canvas.SetTop(line, particle.Y);
                AnimationCanvas.Children.Add(line);
                _particles.Add(particle);
            }
        }

        private void CreateSnowParticles(AnimationState state)
        {
            if (Bounds.Width == 0 || Bounds.Height == 0) return;

            int particleCount = (int)(state.ParticleCount * state.Intensity);
            particleCount = Math.Min(particleCount, MaxParticles);

            for (int i = 0; i < particleCount && _particles.Count < MaxParticles; i++)
            {
                var size = 4 + _random.NextDouble() * 8;
                var ellipse = new Ellipse
                {
                    Fill = new SolidColorBrush(Color.FromArgb(250, 255, 255, 255)),
                    Width = size,
                    Height = size
                };

                var particle = new Particle
                {
                    Element = ellipse,
                    X = _random.NextDouble() * Bounds.Width,
                    Y = _random.NextDouble() * Bounds.Height,
                    VelocityY = 0.8 + _random.NextDouble() * 2 * state.Intensity,
                    VelocityX = Math.Sin(_random.NextDouble() * Math.PI * 2) * 1.5,
                    Rotation = _random.NextDouble() * 360,
                    RotationSpeed = -3 + _random.NextDouble() * 6,
                    Opacity = 0.8
                };

                Canvas.SetLeft(ellipse, particle.X);
                Canvas.SetTop(ellipse, particle.Y);
                AnimationCanvas.Children.Add(ellipse);
                _particles.Add(particle);
            }
        }

        private void CreateLightningFlashes(AnimationState state)
        {
            if (_random.NextDouble() > 0.02) return;

            var flash = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(180, 255, 255, 200)),
                Width = Bounds.Width,
                Height = Bounds.Height
            };

            AnimationCanvas.Children.Add(flash);

            var opacity = 0.4;
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            timer.Tick += (s, e) =>
            {
                opacity -= 0.08;
                flash.Opacity = Math.Max(0, opacity);
                if (opacity <= 0)
                {
                    timer.Stop();
                    AnimationCanvas.Children.Remove(flash);
                }
            };
            timer.Start();
        }

        private void CreateFogLayers(AnimationState state)
        {
            if (Bounds.Width == 0 || Bounds.Height == 0) return;

            for (int i = 0; i < 4; i++)
            {
                var fogLayer = new Rectangle
                {
                    Width = Bounds.Width * 1.3,
                    Height = 80 + _random.NextDouble() * 120,
                    Fill = new SolidColorBrush(Color.FromArgb(50, 220, 220, 220)),
                    Opacity = 0.4 + _random.NextDouble() * 0.3
                };

                var particle = new Particle
                {
                    Element = fogLayer,
                    X = -Bounds.Width * 0.3,
                    Y = _random.NextDouble() * Bounds.Height,
                    VelocityX = 5 + _random.NextDouble() * 12,
                    VelocityY = 0
                };

                Canvas.SetLeft(fogLayer, particle.X);
                Canvas.SetTop(fogLayer, particle.Y);
                AnimationCanvas.Children.Add(fogLayer);
                _particles.Add(particle);
            }
        }

        private void ClearAllAnimations()
        {
            AnimationCanvas.Children.Clear();
            _particles.Clear();
            _clouds.Clear();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (_currentState == null || Bounds.Width == 0 || Bounds.Height == 0)
                return;

            for (int i = _clouds.Count - 1; i >= 0; i--)
            {
                var cloud = _clouds[i];
                cloud.X += cloud.Speed * 0.016;

                if (cloud.X > Bounds.Width + 200)
                {
                    cloud.X = -150;
                    cloud.Y = _random.NextDouble() * (Bounds.Height * 0.5);
                }

                Canvas.SetLeft(cloud.Element, cloud.X);
                Canvas.SetTop(cloud.Element, cloud.Y);
            }

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var particle = _particles[i];
                particle.Y += particle.VelocityY;
                particle.X += particle.VelocityX;

                if (_currentState.Condition == WeatherCondition.Snowy)
                {
                    particle.VelocityX = Math.Sin(particle.Y * 0.04) * 1.8;
                    particle.Rotation += particle.RotationSpeed;
                    if (particle.Element is Ellipse ellipse)
                        particle.Element.RenderTransform = new RotateTransform(particle.Rotation);
                }

                if (_currentState.Condition == WeatherCondition.Sunny)
                {
                    particle.LifeTime--;
                    if (particle.LifeTime <= 0)
                    {
                        AnimationCanvas.Children.Remove(particle.Element);
                        _particles.RemoveAt(i);
                        continue;
                    }
                }

                if (_currentState.Condition == WeatherCondition.Foggy)
                {
                    if (particle.X > Bounds.Width + 100)
                    {
                        particle.X = -Bounds.Width * 0.3;
                        particle.Y = _random.NextDouble() * Bounds.Height;
                    }
                }
                else
                {
                    if (particle.Y > Bounds.Height + 30)
                    {
                        particle.Y = -30;
                        particle.X = _random.NextDouble() * Bounds.Width;
                    }
                    if (particle.X < -50) particle.X = Bounds.Width + 50;
                    else if (particle.X > Bounds.Width + 50) particle.X = -50;
                }

                Canvas.SetLeft(particle.Element, particle.X);
                Canvas.SetTop(particle.Element, particle.Y);

                if (Math.Abs(particle.Opacity - 1.0) > OptimizationThreshold)
                    particle.Element.Opacity = particle.Opacity;
            }
        }

        private class Particle
        {
            public Control Element { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double VelocityX { get; set; }
            public double VelocityY { get; set; }
            public double Rotation { get; set; }
            public double RotationSpeed { get; set; }
            public int LifeTime { get; set; }
            public double Opacity { get; set; } = 1.0;
        }

        private class Cloud
        {
            public Control Element { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Speed { get; set; }
            public double Scale { get; set; }
            public double Opacity { get; set; }
            public CloudType Type { get; set; }
            public int Depth { get; set; }
        }

        private enum CloudType { Wispy, Puffy, Cumulus }
    }
}