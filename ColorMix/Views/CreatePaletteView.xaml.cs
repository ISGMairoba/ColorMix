using ColorMix.ViewModel;

namespace ColorMix.Views;

public partial class CreatePaletteView : ContentPage
{
	private CreatePaletteViewModel _viewModel;

	public CreatePaletteView()
	{
		InitializeComponent();
		_viewModel = new CreatePaletteViewModel();
        BindingContext = _viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		// Reset to create mode if no palette ID is being passed
		// The ApplyQueryAttributes will handle edit mode
	}
}