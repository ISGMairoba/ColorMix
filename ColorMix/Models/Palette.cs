
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


    public class MixColor
    {
        public string ColorName { get; set; }
        public Color Color { get; set; }
        public double Percentage { get; set; }
        public double Ratio { get; set; }
        public string DisplayText => $"{Ratio} ({Percentage}%)";

        public MixColor(string colorName, Color color, double percentage, double ratio)
        {
            ColorName = colorName;
            Color = color;
            Percentage = percentage;
            Ratio = ratio;
        }
    }
}
