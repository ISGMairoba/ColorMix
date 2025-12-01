using ColorMix.Data.Entities;

namespace ColorMix.Views;

public partial class ColorSelectionPopup : ContentPage
{
    public ColorEntity SelectedColor { get; private set; }

    public ColorSelectionPopup(List<ColorEntity> colors)
    {
        InitializeComponent();
        ColorsCollection.ItemsSource = colors;
    }

    private async void OnColorSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is ColorEntity selectedColor)
        {
            SelectedColor = selectedColor;
            await Navigation.PopModalAsync();
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        SelectedColor = null;
        await Navigation.PopModalAsync();
    }
}
