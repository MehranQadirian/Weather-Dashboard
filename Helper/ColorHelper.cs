using Avalonia.Media;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Weather.Dashboard.Avalonia.Helper
{
    public static class ColorHelper
    {
        private static readonly Regex RgbaRegex = new Regex(@"rgba?\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*(?:,\s*([\d.]+)\s*)?\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Color ParseColor(string colorString)
        {
            if (string.IsNullOrWhiteSpace(colorString))
                throw new ArgumentException("Color string is empty");

            colorString = colorString.Trim().ToLowerInvariant();
            try
            {
                return Color.Parse(colorString);
            }
            catch {  }
            var match = RgbaRegex.Match(colorString);
            if (match.Success)
            {
                byte r = byte.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                byte g = byte.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                byte b = byte.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                byte a = (match.Groups.Count > 4 && double.TryParse(match.Groups[4].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double alpha))
                    ? (byte)(alpha * 255)
                    : (byte)255;

                return Color.FromArgb(a, r, g, b);
            }

            throw new ArgumentException($"Invalid color string: '{colorString}'");
        }

        public static string ToHexString(Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}