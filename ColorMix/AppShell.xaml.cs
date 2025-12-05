using ColorMix.Constants;

namespace ColorMix
{
    /// <summary>
    /// AppShell is the main navigation container for the application.
    /// It inherits from Shell, which is a MAUI class that provides navigation features.
    /// The visual structure (flyout menu, tabs, etc.) is defined in AppShell.xaml.
    /// </summary>
    public partial class AppShell : Shell
    {
        /// <summary>
        /// Constructor - Sets up navigation routes when the shell is created.
        /// Routes allow us to navigate to pages using URI-style paths like "EditColor" or "CreatePalette".
        /// </summary>
        public AppShell()
        {
            // Load the XAML that defines the shell's visual structure
            InitializeComponent();
            
            // Register routes for navigation
            // These routes are used with Shell.Current.GoToAsync("route") to navigate between pages
            // Format: Routing.RegisterRoute("route_name", typeof(PageToNavigateTo))
            Routing.RegisterRoute(Routes.EditColor, typeof(Views.CreateColorView));
            Routing.RegisterRoute(Routes.CreatePalette, typeof(Views.CreatePaletteView));
        }
    }
}
