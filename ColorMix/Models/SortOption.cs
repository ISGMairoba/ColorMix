/// <summary>
/// This file defines sorting options for colors and palettes.
/// Enums are a way to define a set of named constants, making code more readable
/// than using numbers or strings for these choices.
/// </summary>
namespace ColorMix.Models
{
    /// <summary>
    /// Defines the different ways colors or palettes can be sorted in the UI.
    /// Used in sort dropdowns and sort functions to determine the sorting criteria.
    /// </summary>
    public enum SortOption
    {
        /// <summary>
        /// Sort alphabetically by name (A-Z).
        /// </summary>
        Name,
        
        /// <summary>
        /// Sort by creation date (newest first or oldest first).
        /// </summary>
        DateCreated,
        
        /// <summary>
        /// Sort by the amount of red in the color (0-255).
        /// </summary>
        Red,
        
        /// <summary>
        /// Sort by the amount of green in the color (0-255).
        /// </summary>
        Green,
        
        /// <summary>
        /// Sort by the amount of blue in the color (0-255).
        /// </summary>
        Blue,
        
        /// <summary>
        /// Sort by hue (color position on the color wheel: red, orange, yellow, green, blue, purple).
        /// </summary>
        Hue
    }
}

