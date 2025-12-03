using ColorMix.Constants;

namespace ColorMix
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Register routes for navigation
            Routing.RegisterRoute(Routes.EditColor, typeof(Views.CreateColorView));
            Routing.RegisterRoute(Routes.CreatePalette, typeof(Views.CreatePaletteView));
        }
    }
}
