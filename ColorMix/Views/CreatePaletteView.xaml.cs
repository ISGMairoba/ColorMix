using ColorMix.ViewModel;

namespace ColorMix.Views;

/// <summary>
/// View for creating and editing color palettes.
/// Uses CreatePaletteViewModel for palette management and color mixing logic.
/// </summary>
public partial class CreatePaletteView : ContentPage
{
	private CreatePaletteViewModel _viewModel;

	/// <summary>
	/// Constructor - creates the ViewModel directly (not via DI).
	/// The ViewModel implements IQueryAttributable, so MAUI will automatically
	/// call ApplyQueryAttributes when navigating with parameters like:
	/// Shell.GoToAsync("CreatePalette", new { PaletteId = 5 })
	/// </summary>
	public CreatePaletteView()
	{
		InitializeComponent();  // Load XAML UI
		_viewModel = new CreatePaletteViewModel();
        BindingContext = _viewModel;  // Connect ViewModel to View for data binding
	}

	/// <summary>
	/// Called when the page appears.
	/// The ViewModel's ApplyQueryAttributes handles loading existing palette data if needed.
	/// </summary>
	protected override void OnAppearing()
	{
		base.OnAppearing();
		// Reset to create mode if no palette ID is being passed
		// The ApplyQueryAttributes will handle edit mode
	}

    // This method runs when the user presses the phone's BACK button
    protected override bool OnBackButtonPressed()
    {
        // Check if the page has a ViewModel assigned
        // (BindingContext is basically the ViewModel attached to the page)
        if (BindingContext is CreatePaletteViewModel vm)
        {
            // Call the ViewModel's back-button command
            // This command will decide whether to show a popup, save, or go back
            vm.OnBackButtonPressedCommand.Execute(null);
        }

        // Return TRUE to tell MAUI:
        // "Don't do the normal back action, we already handled it"
        return true;
    }
}