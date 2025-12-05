/// <summary>
/// This file contains helper converter classes.
/// Converters are used in XAML data binding to transform values.
/// For example, converting a boolean to its opposite (true->false, false->true).
/// </summary>
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMix.Helpers
{
    /// <summary>
    /// Helper class for color conversions.
    /// Currently contains CMYK to RGB conversion (not actively used - CreateColorViewModel has its own conversion logic).
    /// </summary>
    class Converters
    {
        /// <summary>
        /// Converts CMYK color values to RGB.
        /// CMYK = Cyan, Magenta, Yellow, Black (used in printing)
        /// RGB = Red, Green, Blue (used in screens)
        /// </summary>
        /// <param name="c">Cyan (0-100)</param>
        /// <param name="m">Magenta (0-100)</param>
        /// <param name="y">Yellow (0-100)</param>
        /// <param name="k">Black (0-100)</param>
        /// <returns>Array of [R, G, B] values (0-255)</returns>
        public int[] CmykToRgb(int c, int m, int y, int k)
        {
            int r, g, b;
            r = (int)Math.Round(255.0 * (1 - c / 100) * (1 - k / 100));
            g = (int)Math.Round(255.0 * (1 - m / 100) * (1 - k / 100));
            b = (int)Math.Round(255.0 * (1 - y / 100) * (1 - k / 100));
            return [r, g, b];
        }
    }

    /// <summary>
    /// Value converter that inverts boolean values.
    /// Used in XAML binding to hide elements when a boolean is true, or show them when false.
    /// 
    /// Example usage in XAML:
    /// IsVisible="{Binding IsSearching, Converter={StaticResource InvertedBoolConverter}}"
    /// This makes the element visible when IsSearching is false.
    /// </summary>
    public class InvertedBoolConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean to its opposite value.
        /// </summary>
        /// <param name="value">The input boolean</param>
        /// <returns>The inverted boolean (true->false, false->true)</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }

        /// <summary>
        /// Converts back - also inverts the boolean.
        /// Used when binding is two-way (rare for this converter).
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }
    }
}
