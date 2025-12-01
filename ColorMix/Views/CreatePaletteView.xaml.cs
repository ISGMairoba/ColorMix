using ColorMix.ViewModel;

namespace ColorMix.Views;

public partial class CreatePaletteView : ContentPage
{
	public CreatePaletteView()
	{
		InitializeComponent();
        BindingContext = new CreatePaletteViewModel();
	}
}