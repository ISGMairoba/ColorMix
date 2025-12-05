/// <summary>
/// This file contains navigation route constants.
/// Using constants instead of hardcoded strings prevents typos and makes navigation more maintainable.
/// When you need to navigate to a page, use these constants instead of typing route names directly.
/// </summary>
namespace ColorMix.Constants
{
    /// <summary>
    /// Navigation route constants for the ColorMix application.
    /// Each constant represents a route that can be used with Shell.GoToAsync() for navigation.
    /// Example: await Shell.Current.GoToAsync(Routes.EditColor);
    /// </summary>
    public static class Routes
    {
        /// <summary>
        /// Route to the Create/Edit Palette page.
        /// Use this when navigating to create a new palette or edit an existing one.
        /// </summary>
        public const string CreatePalette = "CreatePaletteView";
        
        /// <summary>
        /// Route to the Edit Color page.
        /// Use this when navigating to edit an existing color.
        /// </summary>
        public const string EditColor = "EditColor";
    }
}
