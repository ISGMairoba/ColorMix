using System;
using System.Globalization;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace ColorMix {
    public class ColorToRGBConverter : IValueConverter
    {
        // Convert Color to RGB string (for UI)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return $"RGB({(int)(color.Red * 255)}, {(int)(color.Green * 255)}, {(int)(color.Blue * 255)})";
            }
            return "Invalid Color";
        }

        // Convert RGB string back to Color (for ViewModel)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string rgbString)
            {
                var parts = rgbString.Replace("RGB(", "").Replace(")", "").Split(',');
                if (parts.Length == 3 && int.TryParse(parts[0].Trim(), out int r)
                                      && int.TryParse(parts[1].Trim(), out int g)
                                      && int.TryParse(parts[2].Trim(), out int b))
                {
                    return Color.FromRgb(r / 255.0, g / 255.0, b / 255.0);
                }
            }
            return Colors.Black; // Fallback color
        }
    }

}
