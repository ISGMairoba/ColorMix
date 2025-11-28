using ColorMix.ViewModel;

namespace ColorMix.Views;

public partial class ColorsView : ContentPage
{
	private readonly ColorsViewModel _viewModel;

	public ColorsView(ColorsViewModel viewModel)
	{
		_viewModel = viewModel;
		InitializeComponent();
		BindingContext = _viewModel;
	}

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is ColorsViewModel vm)
        {
            // Reset state on a background thread as requested
            Task.Run(() => 
            {
                MainThread.BeginInvokeOnMainThread(() => vm.ResetState());
            });
        }
    }

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.LoadColorsAsync();
	}
}