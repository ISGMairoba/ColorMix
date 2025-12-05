/// <summary>
/// This file is the main entry point of the ColorMix application.
/// The App class inherits from Application and is the first thing that runs when the app starts.
/// </summary>
namespace ColorMix
{
    /// <summary>
    /// The main Application class for ColorMix.
    /// This is where the app begins its lifecycle and sets up the initial page structure.
    /// The "partial" keyword means this class definition is split between this .cs file and the App.xaml file.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Constructor - This runs when the app first starts.
        /// It sets up the basic structure and navigation for the entire application.
        /// </summary>
        public App()
        {
            // InitializeComponent() loads the XAML (user interface definition) from App.xaml
            // This is automatically generated code that connects the XAML to this C# class
            InitializeComponent();

            // Set the main page to AppShell, which is our navigation container
            // AppShell provides the navigation structure (tabs, flyout menu, routing, etc.)
            MainPage = new AppShell();
        }
    }
}
