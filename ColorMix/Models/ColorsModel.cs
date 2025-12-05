/// <summary>
/// This file defines a Color model for the UI layer.
/// Models are different from Entities - Entities map to database tables, while Models are used in the UI.
/// Models often include UI-specific properties and behaviors (like property change notifications).
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ColorMix.Models
{
    /// <summary>
    /// Represents a color in the user interface.
    /// This class implements INotifyPropertyChanged, which is essential for data binding in MAUI.
    /// When a property changes, the UI is automatically notified and updates itself.
    /// </summary>
    public class ColorsModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Unique identifier for this color.
        /// Corresponds to the color's ID in the database.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// The MAUI Color object representing this color.
        /// Used for rendering the color in UI elements.
        /// </summary>
        public Color Color { get; set; }
        
        /// <summary>
        /// Display name for the color (e.g., "Bright Red", "Ocean Blue").
        /// </summary>
        public string ColorName { get; set; }
        
        /// <summary>
        /// Hexadecimal representation of the color (e.g., "#FF5733").
        /// </summary>
        public string HexValue { get; set; }
        
        /// <summary>
        /// When this color was created.
        /// </summary>
        public DateTime DateCreated { get; set; }

        // Backing field for IsSelected property
        private bool _isSelected;
        
        /// <summary>
        /// Indicates whether this color is currently selected in the UI.
        /// When this changes, the UI is notified (via OnPropertyChanged) to update the visual selection state.
        /// This is used for multi-select scenarios where users can select multiple colors.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;  // No change, don't notify
                _isSelected = value;
                OnPropertyChanged();  // Notify UI that IsSelected changed
            }
        }

        /// <summary>
        /// Formatted text showing the RGB values and hex code.
        /// This is a computed property - it doesn't store data, it generates text from other properties.
        /// For example: "rgb(255, 128, 64) - #FF8040"
        /// </summary>
        public string DisplayText => $"rgb({Color.Red *255}, {Color.Green * 255},{Color.Blue * 255}) - {HexValue}";
        
        /// <summary>
        /// Constructor - Creates a new color model with the specified values.
        /// </summary>
        /// <param name="color">The MAUI Color object</param>
        /// <param name="colorName">Display name for the color</param>
        /// <param name="hexValue">Hex code for the color</param>
        public ColorsModel(Color color, string colorName, string hexValue) 
        { 
            this.Color = color;
            this.ColorName = colorName;
            this.HexValue = hexValue;
        }

        /// <summary>
        /// Event that fires when any property changes.
        /// This is part of the INotifyPropertyChanged interface.
        /// MAUI's data binding system listens to this event to update the UI.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        
        /// <summary>
        /// Raises the PropertyChanged event to notify the UI that a property has changed.
        /// The [CallerMemberName] attribute automatically fills in the property name.
        /// For example, calling OnPropertyChanged() from the IsSelected setter automatically
        /// passes "IsSelected" as the propertyName.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed (auto-filled by compiler)</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

