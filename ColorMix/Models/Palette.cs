
namespace ColorMix
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Microsoft.Maui.Graphics;

    public class Palette : INotifyPropertyChanged
    {
        private string _paletteName;
        private string _paletteColorHex;
        private Color _paletteColor;

        public string PaletteName
        {
            get => _paletteName;
            set { _paletteName = value; OnPropertyChanged(); }
        }

        public string PaletteColorHex
        {
            get => _paletteColorHex;
            set { _paletteColorHex = value; OnPropertyChanged(); }
        }

        public Color PaletteColor
        {
            get => _paletteColor;
            set { _paletteColor = value; OnPropertyChanged(); }
        }

        public string DisplayText => $"RGB({(int)(PaletteColor.Red * 255)}, {(int)(PaletteColor.Green * 255)}, {(int)(PaletteColor.Blue * 255)}) {PaletteColorHex}";

        public ObservableCollection<MixColor> PaletteColors { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Palette(string paletteName, string paletteColorHex, Color paletteColor)
        {
            _paletteName = paletteName;
            _paletteColorHex = paletteColorHex;
            _paletteColor = paletteColor;
        }
    }


    public class MixColor : INotifyPropertyChanged
    {
        private double _percentage;
        private double _ratio;

        public string ColorName { get; set; }
        public Color Color { get; set; }

        public double Percentage
        {
            get => _percentage;
            set
            {
                if (_percentage != value)
                {
                    _percentage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayText));
                }
            }
        }

        public double Ratio
        {
            get => _ratio;
            set
            {
                if (_ratio != value)
                {
                    _ratio = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayText));
                }
            }
        }

        public string DisplayText => $"{Ratio} ({Percentage:F1}%)";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MixColor(string colorName, Color color, double percentage, double ratio)
        {
            ColorName = colorName;
            Color = color;
            Percentage = percentage;
            Ratio = ratio;
        }
    }
}
