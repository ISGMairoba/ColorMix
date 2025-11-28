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

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.LoadColorsAsync();
	}
}