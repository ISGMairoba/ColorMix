namespace ColorMix
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("EditColor", typeof(Views.CreateColorView));
        }
    }
}
