using ColorMix.Helpers;
using ColorMix.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ColorMix.ViewModel
{
    public class CreateColorViewModel : INotifyPropertyChanged
    {
        #region Fields
        private int rgbBlue;
        private int rgbGreen;
        private int rgbRed;
        private int cmykBlack;
        private int cmykMagenta;
        private int cmykYellow;
        private int cmykCyan;
        private string _selectedMode = "RGB";
        private readonly ColorDrawable _colorDrawable;
        private readonly IColorService? _colorService;

        // Conversion guards
        private bool _isUpdatingFromRGB;
        private bool _isUpdatingFromCMYK;
        #endregion

        #region Properties
        public int RgbRed
        {
            get => rgbRed;
            set
            {
                if (rgbRed == value) return;
                rgbRed = value;
                UpdateColor();
                if (!_isUpdatingFromCMYK && SelectedMode == "RGB")
                    UpdateCmykSlider();
                OnPropertyChanged();
            }
        }

        public int RgbGreen
        {
            get => rgbGreen;
            set
            {
                if (rgbGreen == value) return;
                rgbGreen = value;
                UpdateColor();
                if (!_isUpdatingFromCMYK && SelectedMode == "RGB")
                    UpdateCmykSlider();
                OnPropertyChanged();
            }
        }

        public int RgbBlue
        {
            get => rgbBlue;
            set
            {
                if (rgbBlue == value) return;
                rgbBlue = value;
                UpdateColor();
                if (!_isUpdatingFromCMYK && SelectedMode == "RGB")
                    UpdateCmykSlider();
                OnPropertyChanged();
            }
        }

        public int CmykCyan
        {
            get => cmykCyan;
            set
            {
                if (cmykCyan == value) return;
                cmykCyan = value;
                if (!_isUpdatingFromRGB) ConvertToRGB();
                OnPropertyChanged();
            }
        }

        public int CmykMagenta
        {
            get => cmykMagenta;
            set
            {
                if (cmykMagenta == value) return;
                cmykMagenta = value;
                if (!_isUpdatingFromRGB) ConvertToRGB();
                OnPropertyChanged();
            }
        }

        public int CmykYellow
        {
            get => cmykYellow;
            set
            {
                if (cmykYellow == value) return;
                cmykYellow = value;
                if (!_isUpdatingFromRGB) ConvertToRGB();
                OnPropertyChanged();
            }
        }

        public int CmykBlack
        {
            get => cmykBlack;
            set
            {
                if (cmykBlack == value) return;
                cmykBlack = value;
                if (!_isUpdatingFromRGB) ConvertToRGB();
                OnPropertyChanged();
            }
        }

        public string SelectedMode
        {
            get => _selectedMode;
            set
            {
                if (_selectedMode == value) return;

                _selectedMode = value;

                if (_selectedMode == "CMYK")
                {
                    _isUpdatingFromRGB = true;
                    UpdateCmykSlider();
                    _isUpdatingFromRGB = false;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRGBVisible));
                OnPropertyChanged(nameof(IsCMYKVisible));
            }
        }

        public bool IsRGBVisible => SelectedMode == "RGB";
        public bool IsCMYKVisible => SelectedMode == "CMYK";
        public ColorDrawable ColorDrawable => _colorDrawable;
        public ICommand ChangeModeCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ResetCommand { get; }

        private string _colorName = string.Empty;
        public string ColorName
        {
            get => _colorName;
            set
            {
                if (_colorName == value) return;
                _colorName = value;
                OnPropertyChanged();
                ((Command)SaveCommand).ChangeCanExecute();
            }
        }

        public int ColorId { get; set; }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        public CreateColorViewModel()
        {
            ChangeModeCommand = new Command<string>(ChangeMode);
            SaveCommand = new Command(async () => await SaveColorAsync(), CanSave);
            ResetCommand = new Command(ResetColor);
            _colorDrawable = new ColorDrawable();
            UpdateColor();
        }

        // Constructor for DI
        public CreateColorViewModel(IColorService colorService) : this()
        {
            _colorService = colorService;
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(ColorName);
        }

        private async Task SaveColorAsync()
        {
            if (_colorService == null) return;

            try
            {
                var hexValue = $"#{RgbRed:X2}{RgbGreen:X2}{RgbBlue:X2}";

                if (ColorId > 0)
                {
                    // Update existing color
                    var existingColor = await _colorService.GetColorByIdAsync(ColorId);
                    if (existingColor != null)
                    {
                        existingColor.ColorName = ColorName;
                        existingColor.HexValue = hexValue;
                        existingColor.Red = RgbRed;
                        existingColor.Green = RgbGreen;
                        existingColor.Blue = RgbBlue;
                        await _colorService.UpdateColorAsync(existingColor);
                    }
                }
                else
                {
                    // Create new color
                    var newColor = new Data.Entities.ColorEntity
                    {
                        ColorName = ColorName,
                        HexValue = hexValue,
                        Red = RgbRed,
                        Green = RgbGreen,
                        Blue = RgbBlue
                    };
                    await _colorService.AddColorAsync(newColor);
                }

                // Navigate back
                await Shell.Current.GoToAsync("//ColorsView");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save color: {ex.Message}", "OK");
            }
        }

        private void ResetColor()
        {
            _isUpdatingFromCMYK = true;
            RgbRed = 0;
            RgbGreen = 0;
            RgbBlue = 0;
            ColorName = string.Empty;
            _isUpdatingFromCMYK = false;
            UpdateColor();
            UpdateCmykSlider();
        }

        public void LoadColorData(int red, int green, int blue)
        {
            _isUpdatingFromCMYK = true;
            RgbRed = red;
            RgbGreen = green;
            RgbBlue = blue;
            _isUpdatingFromCMYK = false;
            UpdateColor();
            UpdateCmykSlider();
        }

        public void LoadColorData(int colorId, string colorName, int red, int green, int blue)
        {
            ColorId = colorId;
            ColorName = colorName;
            LoadColorData(red, green, blue);
        }

        private void ChangeMode(string mode) => SelectedMode = mode;

        private void UpdateColor()
        {
            var newColor = Color.FromRgb(RgbRed, RgbGreen, RgbBlue);
            _colorDrawable.SetColor(newColor);
            OnPropertyChanged(nameof(ColorDrawable));
        }

        private void ConvertToRGB()
        {
            if (_isUpdatingFromRGB) return;

            _isUpdatingFromCMYK = true;

            double c = Math.Clamp(CmykCyan / 100.0, 0, 1);
            double m = Math.Clamp(CmykMagenta / 100.0, 0, 1);
            double y = Math.Clamp(CmykYellow / 100.0, 0, 1);
            double k = Math.Clamp(CmykBlack / 100.0, 0, 1);

            RgbRed = (int)Math.Round(255 * (1 - c) * (1 - k));
            RgbGreen = (int)Math.Round(255 * (1 - m) * (1 - k));
            RgbBlue = (int)Math.Round(255 * (1 - y) * (1 - k));

            _isUpdatingFromCMYK = false;
            UpdateColor();
        }

        private void UpdateCmykSlider()
        {
            if (_isUpdatingFromCMYK) return;

            _isUpdatingFromRGB = true;

            if (RgbRed == 0 && RgbGreen == 0 && RgbBlue == 0)
            {
                CmykCyan = 0;
                CmykMagenta = 0;
                CmykYellow = 0;
                CmykBlack = 100;
                _isUpdatingFromRGB = false;
                return;
            }

            double r = RgbRed / 255.0;
            double g = RgbGreen / 255.0;
            double b = RgbBlue / 255.0;

            double k = 1 - Math.Max(r, Math.Max(g, b));
            double invK = 1 - k;

            CmykCyan = (int)Math.Round(100 * (1 - r - k) / invK);
            CmykMagenta = (int)Math.Round(100 * (1 - g - k) / invK);
            CmykYellow = (int)Math.Round(100 * (1 - b - k) / invK);
            CmykBlack = (int)Math.Round(100 * k);

            _isUpdatingFromRGB = false;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
