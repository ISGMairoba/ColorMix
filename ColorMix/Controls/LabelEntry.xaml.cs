using System.Runtime.CompilerServices;

namespace ColorMix.Controls;

public partial class LabelEntry : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(LabelEntry), default(string), BindingMode.TwoWay, propertyChanged: OnTextPropertyChanged);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(LabelEntry), Colors.Black);

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(nameof(FontSize), typeof(double), typeof(LabelEntry), 14.0);

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(LabelEntry), string.Empty, propertyChanged: OnPlaceholderPropertyChanged);

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    private static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LabelEntry control)
        {
            control.UpdateDisplayText();
        }
    }

    private static void OnPlaceholderPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LabelEntry control)
        {
            control.UpdateDisplayText();
        }
    }

    private void UpdateDisplayText()
    {
        if (string.IsNullOrEmpty(Text))
        {
            DisplayLabel.Text = Placeholder;
            DisplayLabel.Opacity = 0.5;
        }
        else
        {
            DisplayLabel.Text = Text;
            DisplayLabel.Opacity = 1.0;
        }
    }

    public LabelEntry()
    {
        InitializeComponent();
        UpdateDisplayText();
    }

    private void OnLabelTapped(object sender, EventArgs e)
    {
        DisplayLabel.IsVisible = false;
        EditEntry.IsVisible = true;
        EditEntry.Focus();
    }

    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {
        ShowLabel();
    }

    private void OnEntryCompleted(object sender, EventArgs e)
    {
        ShowLabel();
    }

    private void ShowLabel()
    {
        EditEntry.IsVisible = false;
        DisplayLabel.IsVisible = true;
    }
}
