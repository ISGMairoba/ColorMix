/// <summary>
/// This file contains font icon constants.
/// Font icons are characters from icon fonts (like FontAwesome or Material Icons).
/// Instead of using image files, we use special unicode characters that render as icons.
/// This keeps the app lightweight and icons scalable.
/// </summary>
namespace ColorMix.Helpers
{
    /// <summary>
    /// Constants for font icon unicode characters.
    /// Each constant maps to a specific icon in the app's icon font.
    /// Used in XAML like: Text="{x:Static helpers:FontIcons.Search}"
    /// </summary>
    public static class FontIcons
    {
        /// <summary>Selection/checkbox icon - used for selection mode</summary>
        public const string Selection = "\ue802";
        
        /// <summary>Sort icon - used for sorting options</summary>
        public const string Sort = "\uf161";
        
        /// <summary>Search icon - used for search functionality</summary>
        public const string Search = "\ue803";
        
        /// <summary>Cancel/close icon for search - used to close search bar</summary>
        public const string SearchCancel = "\ue80e";
        
        /// <summary>Delete/trash icon - used for delete operations</summary>
        public const string Delete = "\uf1f8";
        
        /// <summary>Copy/duplicate icon - used for duplication operations</summary>
        public const string Copy = "\uf0c5";
        
        /// <summary>Edit/pencil icon - used for edit operations</summary>
        public const string Edit = "\ue80c";
        
        /// <summary>Share icon - used for sharing functionality</summary>
        public const string Share = "\uf1e0";
        
        /// <summary>Close/X icon - used for closing dialogs or cancel actions</summary>
        public const string Close = "\xf00d";
        
        /// <summary>Save palette icon - used when saving palettes</summary>
        public const string SavePalette = "\ue807";
        
        /// <summary>Add color icon - used to add colors to palette</summary>
        public const string AddColor = "\uf1fb";
        
        /// <summary>Remove color icon - used to remove colors from palette</summary>
        public const string RemoveColor = "\ue80d";
    }
}
