using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace Weather.Dashboard.Avalonia.Controls
{
    public partial class SimpleLineChart : UserControl
    {
        public static readonly StyledProperty<IEnumerable<double>> DataPointsProperty =
            AvaloniaProperty.Register<SimpleLineChart, IEnumerable<double>>(nameof(DataPoints));

        public IEnumerable<double> DataPoints
        {
            get => GetValue(DataPointsProperty);
            set => SetValue(DataPointsProperty, value);
        }

        private Popup _tooltip;
        private Border _tooltipBorder;
        private StackPanel _tooltipContent;
        private List<(Ellipse circle, Ellipse outer, double temp, int index, double x, double y)> _dataPoints;
        private int _currentHoveredIndex = -1;

        public SimpleLineChart()
        {
            InitializeComponent();
            InitializeTooltip();
            _dataPoints = new List<(Ellipse, Ellipse, double, int, double, double)>();
            
            // Add pointer moved event to canvas for smart tooltip
            ChartCanvas.PointerMoved += OnCanvasPointerMoved;
            ChartCanvas.PointerExited += OnCanvasPointerExited;
        }

        private void InitializeTooltip()
        {
            _tooltipContent = new StackPanel
            {
                Spacing = 8,
                Background = Brushes.Transparent
            };

            var contentBorder = new Border
            {
                Background = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                    {
                        new GradientStop(Color.FromArgb(255, 37, 99, 235), 0),
                        new GradientStop(Color.FromArgb(255, 124, 58, 237), 1)
                    }
                },
                CornerRadius = new CornerRadius(16),
                Padding = new Thickness(18, 14),
                Child = _tooltipContent,
                ClipToBounds = true // ✅ اضافه شد تا محتوا از گوشه‌ها بیرون نزند
            };

            _tooltipBorder = new Border
            {
                Background = Brushes.Transparent,
                CornerRadius = new CornerRadius(16), // ✅ همان radius گوشه‌ها
                BoxShadow = new BoxShadows(
                    new BoxShadow
                    {
                        Blur = 32,
                        Spread = 0,
                        Color = Color.FromArgb(60, 0, 0, 0),
                        OffsetY = 8
                    }),
                Child = contentBorder,
                Opacity = 0,
                ClipToBounds = true // ✅ اضافه شد
            };

            _tooltip = new Popup
            {
                Child = _tooltipBorder,
                PlacementMode = PlacementMode.Pointer,
                IsLightDismissEnabled = false,
                PlacementTarget = this,
                HorizontalOffset = 15,
                VerticalOffset = -80
            };
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == DataPointsProperty || change.Property == BoundsProperty)
            {
                DrawChart();
            }
        }

        private void DrawChart()
        {
            ChartCanvas.Children.Clear();
            _dataPoints.Clear();

            if (DataPoints == null || !DataPoints.Any() || Bounds.Width == 0 || Bounds.Height == 0)
                return;

            var points = DataPoints.ToList();
            if (points.Count < 2)
                return;

            double minValue = points.Min();
            double maxValue = points.Max();
            double range = maxValue - minValue;
            if (range == 0) range = 1;

            double width = Bounds.Width;
            double height = Bounds.Height;
            double stepX = width / (points.Count - 1);
            double padding = 20;

            DrawGridLines(width, height, padding, maxValue, range);
            DrawFillArea(points, width, height, padding, minValue, range, stepX);
            DrawLine(points, height, padding, minValue, range, stepX);
            DrawDataPoints(points, height, padding, minValue, range, stepX);
            DrawTimeLabels(points, height, stepX);
        }

        private void DrawGridLines(double width, double height, double padding, double maxValue, double range)
        {
            for (int i = 0; i <= 4; i++)
            {
                double y = padding + (height - 2 * padding) * i / 4;

                var gridLine = new Line
                {
                    StartPoint = new Point(0, y),
                    EndPoint = new Point(width, y),
                    Stroke = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                    StrokeThickness = 1,
                    StrokeDashArray = new AvaloniaList<double> { 5, 3 }
                };
                ChartCanvas.Children.Add(gridLine);

                double temp = maxValue - (range * i / 4);
                var label = new TextBlock
                {
                    Text = $"{temp:F0}°",
                    Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                    FontSize = 11,
                    FontWeight = FontWeight.Medium
                };
                Canvas.SetLeft(label, 5);
                Canvas.SetTop(label, y - 10);
                ChartCanvas.Children.Add(label);
            }
        }

        private void DrawFillArea(List<double> points, double width, double height, double padding, 
            double minValue, double range, double stepX)
        {
            var fillPoints = new List<Point>();
            fillPoints.Add(new Point(0, height - padding));

            for (int i = 0; i < points.Count; i++)
            {
                double x = i * stepX;
                double y = padding + (height - 2 * padding) - 
                    ((points[i] - minValue) / range * (height - 2 * padding));
                fillPoints.Add(new Point(x, y));
            }

            fillPoints.Add(new Point(width, height - padding));

            var fillPolygon = new Polygon
            {
                Points = new AvaloniaList<Point>(fillPoints),
                Fill = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                    {
                        new GradientStop(Color.FromArgb(100, 255, 255, 255), 0),
                        new GradientStop(Color.FromArgb(20, 255, 255, 255), 1)
                    }
                }
            };
            ChartCanvas.Children.Add(fillPolygon);
        }

        private void DrawLine(List<double> points, double height, double padding, 
            double minValue, double range, double stepX)
        {
            var polylinePoints = new List<Point>();
            for (int i = 0; i < points.Count; i++)
            {
                double x = i * stepX;
                double y = padding + (height - 2 * padding) - 
                    ((points[i] - minValue) / range * (height - 2 * padding));
                polylinePoints.Add(new Point(x, y));
            }

            var polyline = new Polyline
            {
                Points = new AvaloniaList<Point>(polylinePoints),
                Stroke = Foreground,
                StrokeThickness = 3.5,
                StrokeJoin = PenLineJoin.Round,
                StrokeLineCap = PenLineCap.Round
            };
            ChartCanvas.Children.Add(polyline);
        }

        private void DrawDataPoints(List<double> points, double height, double padding, 
            double minValue, double range, double stepX)
        {
            var now = DateTime.Now;

            for (int i = 0; i < points.Count; i++)
            {
                double x = i * stepX;
                double y = padding + (height - 2 * padding) - 
                    ((points[i] - minValue) / range * (height - 2 * padding));

                var outerCircle = new Ellipse
                {
                    Width = 12,
                    Height = 12,
                    Fill = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(outerCircle, x - 6);
                Canvas.SetTop(outerCircle, y - 6);
                ChartCanvas.Children.Add(outerCircle);

                var ellipse = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = Foreground,
                    Stroke = new SolidColorBrush(Colors.White),
                    StrokeThickness = 2,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(ellipse, x - 4);
                Canvas.SetTop(ellipse, y - 4);
                ChartCanvas.Children.Add(ellipse);

                _dataPoints.Add((ellipse, outerCircle, points[i], i, x, y));

                if (i % 6 == 0 || i == points.Count - 1)
                {
                    var tempLabel = new TextBlock
                    {
                        Text = $"{points[i]:F0}°",
                        Foreground = Foreground,
                        FontSize = 11,
                        FontWeight = FontWeight.Bold,
                        Padding = new Thickness(6, 3, 6, 3)
                    };

                    var labelBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
                        CornerRadius = new CornerRadius(6),
                        Child = tempLabel
                    };
                    Canvas.SetLeft(labelBorder, x - 18);
                    Canvas.SetTop(labelBorder, y - 30);
                    ChartCanvas.Children.Add(labelBorder);
                }
            }
        }

        private void DrawTimeLabels(List<double> points, double height, double stepX)
        {
            var timeLabels = new[] { "Now", "6h", "12h", "18h", "24h" };
            var timePositions = new[] { 0, points.Count / 4, points.Count / 2, 3 * points.Count / 4, points.Count - 1 };

            for (int i = 0; i < timeLabels.Length; i++)
            {
                var timeLabel = new TextBlock
                {
                    Text = timeLabels[i],
                    Foreground = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)),
                    FontSize = 11,
                    FontWeight = FontWeight.SemiBold
                };
                double xPos = timePositions[i] * stepX;
                Canvas.SetLeft(timeLabel, xPos - 15);
                Canvas.SetTop(timeLabel, height - 15);
                ChartCanvas.Children.Add(timeLabel);
            }
        }

        private void OnCanvasPointerMoved(object sender, PointerEventArgs e)
        {
            if (_dataPoints.Count == 0) return;

            var position = e.GetPosition(ChartCanvas);
            
            // Find the closest point to mouse position
            double minDistance = double.MaxValue;
            int closestIndex = -1;

            for (int i = 0; i < _dataPoints.Count; i++)
            {
                var point = _dataPoints[i];
                double distance = Math.Sqrt(
                    Math.Pow(position.X - point.x, 2) + 
                    Math.Pow(position.Y - point.y, 2)
                );

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }

            // Only show tooltip if mouse is within reasonable distance (e.g., 50 pixels)
            if (minDistance < 50 && closestIndex != -1)
            {
                if (_currentHoveredIndex != closestIndex)
                {
                    // Reset previous point if different
                    if (_currentHoveredIndex >= 0 && _currentHoveredIndex < _dataPoints.Count)
                    {
                        var prevPoint = _dataPoints[_currentHoveredIndex];
                        AnimatePointScale(prevPoint.circle, 1.0);
                        AnimatePointScale(prevPoint.outer, 1.0);
                    }

                    _currentHoveredIndex = closestIndex;
                    var currentPoint = _dataPoints[closestIndex];
                    
                    ShowTooltip(currentPoint.temp, currentPoint.index, DateTime.Now, currentPoint.x, currentPoint.y);
                    AnimatePointScale(currentPoint.circle, 1.3);
                    AnimatePointScale(currentPoint.outer, 1.5);
                }
            }
            else
            {
                // Mouse too far from any point
                if (_currentHoveredIndex >= 0)
                {
                    var prevPoint = _dataPoints[_currentHoveredIndex];
                    AnimatePointScale(prevPoint.circle, 1.0);
                    AnimatePointScale(prevPoint.outer, 1.0);
                    _currentHoveredIndex = -1;
                    HideTooltip();
                }
            }
        }

        private void OnCanvasPointerExited(object sender, PointerEventArgs e)
        {
            if (_currentHoveredIndex >= 0 && _currentHoveredIndex < _dataPoints.Count)
            {
                var prevPoint = _dataPoints[_currentHoveredIndex];
                AnimatePointScale(prevPoint.circle, 1.0);
                AnimatePointScale(prevPoint.outer, 1.0);
            }
            _currentHoveredIndex = -1;
            HideTooltip();
        }

        private void ShowTooltip(double temp, int index, DateTime baseTime, double pointX, double pointY)
        {
            _tooltipContent.Children.Clear();

            var timeOffset = baseTime.AddHours(index);
            
            var timeIcon = new TextBlock
            {
                Text = "🕐",
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 4)
            };
            _tooltipContent.Children.Add(timeIcon);

            var timeText = new TextBlock
            {
                Text = timeOffset.ToString("HH:mm"),
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeight.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _tooltipContent.Children.Add(timeText);

            var separator = new Border
            {
                Height = 1,
                Background = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
                Margin = new Thickness(8, 8, 8, 8)
            };
            _tooltipContent.Children.Add(separator);

            var tempStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var tempIcon = new TextBlock
            {
                Text = "🌡️",
                FontSize = 20,
                VerticalAlignment = VerticalAlignment.Center
            };
            tempStack.Children.Add(tempIcon);

            var tempText = new TextBlock
            {
                Text = $"{temp:F1}°",
                Foreground = Brushes.White,
                FontSize = 28,
                FontWeight = FontWeight.Bold,
                VerticalAlignment = VerticalAlignment.Center
            };
            tempStack.Children.Add(tempText);

            _tooltipContent.Children.Add(tempStack);

            var hourLabel = new TextBlock
            {
                Text = index == 0 ? "Current" : $"+{index}h",
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                FontSize = 12,
                FontWeight = FontWeight.Medium,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 6, 0, 0)
            };
            _tooltipContent.Children.Add(hourLabel);

            if (!_tooltip.IsOpen)
            {
                _tooltip.IsOpen = true;
                AnimateTooltipIn();
            }
        }

        private async void AnimateTooltipIn()
        {
            _tooltipBorder.Opacity = 0;
            _tooltipBorder.RenderTransform = new ScaleTransform(0.9, 0.9);
            _tooltipBorder.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
    
            var opacityTransition = new DoubleTransition
            {
                Property = Border.OpacityProperty,
                Duration = TimeSpan.FromMilliseconds(200),
                Easing = new CubicEaseOut()
            };
    
            _tooltipBorder.Transitions = new Transitions { opacityTransition };
            _tooltipBorder.Opacity = 1.0;
    
            await AnimateScale(_tooltipBorder, 0.9, 1.0, 200);
        }

        private async void AnimatePointScale(Ellipse ellipse, double scale)
        {
            ellipse.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
            await AnimateScale(ellipse, ellipse.RenderTransform is ScaleTransform st ? st.ScaleX : 1.0, scale, 200);
        }

        private async void HideTooltip()
        {
            if (!_tooltip.IsOpen) return;
            
            await AnimateScale(_tooltipBorder, 1.0, 0.9, 150);
    
            var opacityTransition = new DoubleTransition
            {
                Property = Border.OpacityProperty,
                Duration = TimeSpan.FromMilliseconds(150),
                Easing = new CubicEaseIn()
            };
    
            _tooltipBorder.Transitions = new Transitions { opacityTransition };
            _tooltipBorder.Opacity = 0;
    
            await Task.Delay(150);
            _tooltip.IsOpen = false;
        }

        private async Task AnimateScale(Control control, double fromScale, double toScale, int durationMs)
        {
            var steps = 20;
            var delay = durationMs / steps;
    
            for (int i = 0; i <= steps; i++)
            {
                var progress = (double)i / steps;
                var easedProgress = new CubicEaseOut().Ease(progress);
                var currentScale = fromScale + (toScale - fromScale) * easedProgress;
                control.RenderTransform = new ScaleTransform(currentScale, currentScale);
                await Task.Delay(delay);
            }
        }
    }
}