/// <summary>
/// This file is the code-behind for ColorsView.xaml.
/// Code-behind files contain C# code that supports the XAML UI.
/// This handles page lifecycle events and connects the View to its ViewModel.
/// </summary>
using ColorMix.ViewModel;

namespace ColorMix.Views;

/// <summary>
/// View for displaying and managing the list of saved colors.
/// Uses ColorsViewModel for all UI logic (MVVM pattern).
/// </summary>
public partial class ColorsView : ContentPage
{
	private readonly ColorsViewModel _viewModel;

	/// <summary>
	/// Constructor - receives ViewModel via dependency injection.
	/// Sets up the binding context so XAML can bind to ViewModel properties.
	/// </summary>
	/// <param name="viewModel">The ViewModel for this View</param>
	public ColorsView(ColorsViewModel viewModel)
	{
		_viewModel = viewModel;
		InitializeComponent();  // Loads the XAML UI
		BindingContext = _viewModel;  // Connect ViewModel to View for data binding
	}

	/// <summary>
	/// Called when the page is about to disappear (user navigates away).
	/// Resets search and selection state to provide a clean experience when returning.
	/// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is ColorsViewModel vm)
        {
            // Reset state on a background thread to avoid blocking UI
            Task.Run(() => 
            {
                MainThread.BeginInvokeOnMainThread(() => vm.ResetState());
            });
        }
    }

	/// <summary>
	/// Called when the page appears (becomes visible).
	/// Loads/refreshes the color list from the database.
	/// </summary>
	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.LoadColorsAsync();
	}
}