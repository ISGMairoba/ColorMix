using ColorMix.ViewModel;

namespace ColorMix.Views;

/// <summary>
/// View for creating new colors or editing existing ones.
/// 
/// NAVIGATION PARAMETERS (received when editing):
/// QueryProperty attributes automatically populate these properties from navigation parameters.
/// For example: Shell.GoToAsync("EditColor", new { ColorId = 5, ColorName = "Red", Red = 255, ... })
/// </summary>
[QueryProperty(nameof(ColorId), "ColorId")]
[QueryProperty(nameof(ColorName), "ColorName")]
[QueryProperty(nameof(Red), "Red")]
[QueryProperty(nameof(Green), "Green")]
[QueryProperty(nameof(Blue), "Blue")]
[QueryProperty(nameof(HexValue), "HexValue")]
public partial class CreateColorView : ContentPage
{
	// Navigation parameters - automatically populated when navigating to edit a color
	public int ColorId { get; set; }
	public string ColorName { get; set; } = string.Empty;
	public int Red { get; set; }
	public int Green { get; set; }
	public int Blue { get; set; }
	public string HexValue { get; set; } = string.Empty;

    private readonly CreateColorViewModel _viewModel;

	/// <summary>
	/// Constructor - receives ViewModel via dependency injection.
	/// Also subscribes to ViewModel property changes to invalidate the color preview.
	/// </summary>
	public CreateColorView(CreateColorViewModel viewModel)
	{
		InitializeComponent();  // Load XAML UI
        _viewModel = viewModel;
        BindingContext = _viewModel;
        // Listen for color changes to refresh the preview
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
	}

	/// <summary>
	/// Listens for changes to color values and refreshes the color preview.
	/// When RGB or CMYK values change, we need to tell the GraphicsView to redraw.
	/// </summary>
    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Check if any color-related property changed
        if (e.PropertyName == nameof(CreateColorViewModel.ColorDrawable) ||
            e.PropertyName == nameof(CreateColorViewModel.RgbRed) ||
            e.PropertyName == nameof(CreateColorViewModel.RgbGreen) ||
            e.PropertyName == nameof(CreateColorViewModel.RgbBlue) ||
            e.PropertyName == nameof(CreateColorViewModel.CmykCyan) ||
            e.PropertyName == nameof(CreateColorViewModel.CmykMagenta) ||
            e.PropertyName == nameof(CreateColorViewModel.CmykYellow) ||
            e.PropertyName == nameof(CreateColorViewModel.CmykBlack))
        {
            ColorPreview.Invalidate();  // Redraw the color preview
        }
    }

	/// <summary>
	/// Called when the page appears.
	/// If we have a ColorId > 0, we're in edit mode, so load the color data into the ViewModel.
	/// </summary>
	protected override void OnAppearing()
	{
		base.OnAppearing();

		// If we have color data (editing mode), populate the view model
		if (ColorId > 0)
		{
			_viewModel.LoadColorData(ColorId, ColorName, Red, Green, Blue);
		}
	}

    protected override bool OnBackButtonPressed()
    {
        // Check if the page has a ViewModel assigned
        // (BindingContext is basically the ViewModel attached to the page)
        if (BindingContext is CreateColorViewModel vm)
        {
            // Call the ViewModel's back-button command
            // This command will decide whether to show a popup, save, or go back
            vm.OnBackButtonPressedCommand.Execute(null);
        }

        // Return TRUE to tell MAUI:
        // "Don't do the normal back action, we already handled it"
        return true;
    }

    /// <summary>
    /// Event handler for slider value changes.
    /// Refreshes the color preview when sliders are moved.
    /// </summary>
    private void ColorChange(object sender, ValueChangedEventArgs e)
    {
        ColorPreview.Invalidate();
    }
}