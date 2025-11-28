using ColorMix.ViewModel;

namespace ColorMix.Views;

[QueryProperty(nameof(ColorId), "ColorId")]
[QueryProperty(nameof(ColorName), "ColorName")]
[QueryProperty(nameof(Red), "Red")]
[QueryProperty(nameof(Green), "Green")]
[QueryProperty(nameof(Blue), "Blue")]
[QueryProperty(nameof(HexValue), "HexValue")]
public partial class CreateColorView : ContentPage
{
	public int ColorId { get; set; }
	public string ColorName { get; set; } = string.Empty;
	public int Red { get; set; }
	public int Green { get; set; }
	public int Blue { get; set; }
	public string HexValue { get; set; } = string.Empty;

    private readonly CreateColorViewModel _viewModel;

	public CreateColorView(CreateColorViewModel viewModel)
	{
		InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		// If we have color data (editing mode), populate the view model
		if (ColorId > 0)
		{
			_viewModel.LoadColorData(ColorId, ColorName, Red, Green, Blue);
		}
	}

    private void ColorChange(object sender, ValueChangedEventArgs e)
    {
        ColorPreview.Invalidate();
    }
}