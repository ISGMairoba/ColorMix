
/// <summary>
/// This file defines models for palettes and color mixes.
/// These models are used in the UI layer, separate from database entities.
/// They include UI-specific properties and implement INotifyPropertyChanged for data binding.
/// </summary>
namespace ColorMix
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Microsoft.Maui.Graphics;

    /// <summary>
    /// Represents a color palette or variant in the UI.
    /// A palette can be:
    /// 1. A saved collection of colors
    /// 2. A color mix variant (a specific combination of colors)
    /// Implements INotifyPropertyChanged to support data binding - when properties change,
    /// the UI automatically updates.
    /// </summary>
    public class Palette : INotifyPropertyChanged
    {
        // Backing fields for properties that need change notification
        private string _paletteName;
        private string _paletteColorHex;
        private Color _paletteColor;

        /// <summary>
        /// Database ID of this palette.
        /// Used to identify the palette when loading/saving from the database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the palette (e.g., "Summer Colors", "Brand Palette","Sunset Mix").
        /// When changed, notifies the UI to update any displayed names.
        /// </summary>
        public string PaletteName
        {
            get => _paletteName;
            set { _paletteName = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Hexadecimal color code representing the palette's main or mixed color.
        /// For color mixes, this is the resulting color from mixing components.
        /// Example: "#FF8C42"
        /// </summary>
        public string PaletteColorHex
        {
            get => _paletteColorHex;
            set { _paletteColorHex = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// MAUI Color object for the palette's main or mixed color.
        /// Used for rendering the color in UI elements.
        /// </summary>
        public Color PaletteColor
        {
            get => _paletteColor;
            set { _paletteColor = value; OnPropertyChanged(); }
        }

        // Backing field for IsSelected property
        private bool _isSelected;
        
        /// <summary>
        /// Indicates whether this palette is currently selected in the UI.
        /// Used for multi-select scenarios (e.g., batch operations on palettes).
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Computed property that generates display text showing RGB values and hex code.
        /// For example: "RGB(255, 140, 66) #FF8C42"
        /// The "=>" syntax means this is an expression-bodied property - it calculates the value each time it's accessed.
        /// Returns "N/A" if no color is set.
        /// </summary>
        public string DisplayText => PaletteColor != null 
            ? $"RGB({(int)(PaletteColor.Red * 255)}, {(int)(PaletteColor.Green * 255)}, {(int)(PaletteColor.Blue * 255)}) {PaletteColorHex}"
            : PaletteColorHex ?? "N/A";

        /// <summary>
        /// Collection of colors that make up this palette (for saved palettes) or
        /// mix components (for color variants).
        /// ObservableCollection automatically notifies the UI when items are added/removed.
        /// </summary>
        public ObservableCollection<MixColor> PaletteColors { get; set; } = new();
        
        /// <summary>
        /// Collection of mix variants associated with this palette.
        /// A variant is a specific color created by mixing palette colors in certain proportions.
        /// </summary>
        public ObservableCollection<Palette> Variants { get; set; } = new();

        /// <summary>
        /// Event fired when any property changes.
        /// Part of INotifyPropertyChanged interface - enables data binding.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event to notify the UI of property changes.
        /// [CallerMemberName] automatically fills in the property name.
        /// </summary>
        /// <param name="propertyName">Name of the changed property (auto-filled)</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Constructor - Creates a new palette with the specified values.
        /// </summary>
        /// <param name="paletteName">Name for this palette</param>
        /// <param name="paletteColorHex">Hex code of the palette's color</param>
        /// <param name="paletteColor">MAUI Color object</param>
        public Palette(string paletteName, string paletteColorHex, Color paletteColor)
        {
            _paletteName = paletteName;
            _paletteColorHex = paletteColorHex;
            _paletteColor = paletteColor;
        }
    }


    /// <summary>
    /// Represents a color component in a color mix.
    /// A mix is made up of multiple MixColor components, each with a specific ratio.
    /// For example, "2 parts Red + 1 part Blue" would have:
    /// - One MixColor for Red with Ratio=2
    /// - One MixColor for Blue with Ratio=1
    /// </summary>
    public class MixColor : INotifyPropertyChanged
    {
        // Backing fields for properties that need change notification
        private double _percentage;
        private double _ratio;

        /// <summary>
        /// Name of the color in this mix component (e.g., "Bright Red", "Ocean Blue").
        /// </summary>
        public string ColorName { get; set; }
        
        /// <summary>
        /// The MAUI Color object for this component.
        /// Used for rendering and color calculations.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Percentage of this color in the total mix (0-100).
        /// Calculated as (Ratio / Sum of all Ratios) * 100.
        /// For example, if this component has Ratio=2 and total is 3, Percentage=66.67.
        /// Automatically notifies UI when changed and updates DisplayText.
        /// </summary>
        public double Percentage
        {
            get => _percentage;
            set
            {
                if (_percentage != value)
                {
                    _percentage = value;
                    OnPropertyChanged();  // Notify that Percentage changed
                    OnPropertyChanged(nameof(DisplayText));  // DisplayText depends on Percentage, so notify it changed too
                }
            }
        }

        /// <summary>
        /// Ratio of this color in the mix (e.g., 2 for "2 parts").
        /// This is the relative amount - absolute values don't matter, only proportions.
        /// For example, "2:1" is the same as "20:10" or "4:2".
        /// Automatically notifies UI when changed and updates DisplayText.
        /// </summary>
        public double Ratio
        {
            get => _ratio;
            set
            {
                if (_ratio != value)
                {
                    _ratio = value;
                    OnPropertyChanged();  // Notify that Ratio changed
                    OnPropertyChanged(nameof(DisplayText));  // DisplayText depends on Ratio, so notify it changed too
                }
            }
        }

        /// <summary>
        /// Computed property showing ratio and percentage in user-friendly format.
        /// For example: "2 (66.7%)" means 2 parts, which is 66.7% of the total mix.
        /// </summary>
        public string DisplayText => $"{Ratio} ({Percentage:F1}%)";

        /// <summary>
        /// Event fired when any property changes.
        /// Part of INotifyPropertyChanged interface - enables data binding.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event to notify the UI of property changes.
        /// [CallerMemberName] automatically fills in the property name.
        /// </summary>
        /// <param name="propertyName">Name of the changed property (auto-filled)</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Constructor - Creates a new mix color component with specified values.
        /// </summary>
        /// <param name="colorName">Name of the color</param>
        /// <param name="color">MAUI Color object</param>
        /// <param name="percentage">Percentage in the mix (0-100)</param>
        /// <param name="ratio">Ratio (parts) in the mix</param>
        public MixColor(string colorName, Color color, double percentage, double ratio)
        {
            ColorName = colorName;
            Color = color;
            Percentage = percentage;
            Ratio = ratio;
        }
    }
}
