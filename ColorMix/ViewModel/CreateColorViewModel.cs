/// <summary>
/// This file contains the ViewModel for the Create/Edit Color page.
/// This ViewModel handles:
/// - Creating and editing individual colors
/// - RGB sliders (0-255 for each channel)
/// - CMYK sliders (0-100 percentages for Cyan, Magenta, Yellow, Black)
/// - Converting between RGB and CMYK color spaces
/// - Hex and RGB text entry
/// - Preventing circular updates when converting between color spaces
/// </summary>
using ColorMix.Helpers;
using ColorMix.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ColorMix.ViewModel
{
    /// <summary>
    /// ViewModel for the Create/Edit Color page.
    /// Handles RGB and CMYK color editing with real-time conversion between color spaces.
    /// </summary>
    public class CreateColorViewModel : INotifyPropertyChanged
    {
        #region Fields
        // RGB values (0-255)
        private int rgbBlue;
        private int rgbGreen;
        private int rgbRed;
        
        // CMYK values (0-100 percentages)
        private int cmykBlack;
        private int cmykMagenta;
        private int cmykYellow;
        private int cmykCyan;
        
        private string _selectedMode = "RGB";  // Either "RGB" or "CMYK"
        private readonly ColorDrawable _colorDrawable;  // Custom drawable for previewing color
        private readonly IColorService? _colorService;

        // Conversion guards to prevent circular updates
        // When RGB changes, it updates CMYK. Without guards, CMYK would then update RGB, creating a loop!
        private bool _isUpdatingFromRGB;  // True when converting RGB->CMYK
        private bool _isUpdatingFromCMYK;  // True when converting CMYK->RGB
        #endregion

        #region Properties
        /// <summary>
        /// Red channel value (0-255).
        /// When changed, updates the color preview and CMYK sliders.
        /// </summary>
        public int RgbRed
        {
            get => rgbRed;
            set
            {
                if (rgbRed == value) return;
                rgbRed = value;
                UpdateColor();  // Update color preview
                // Only update CMYK if we're in RGB mode and not already updating from CMYK
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
        public ICommand OnBackButtonPressedCommand { get; } // This command will be triggered when the user clicks BACK

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

        public event PropertyChangedEventHandler? PropertyChanged;

        public CreateColorViewModel()
        {
            ChangeModeCommand = new Command<string>(ChangeMode);
            SaveCommand = new Command(async () => await SaveColorAsync(), CanSave);
            ResetCommand = new Command(ResetColor);
            OnBackButtonPressedCommand = new Command(async () => await HandleBackPress());
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
                string message = "";

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
                        message = "Color updated successfully";
                    }
                    
                    await Toast.Make(message).Show();
                    // Navigate back only when editing
                    await Shell.Current.GoToAsync("..");
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
                    message = "Color saved successfully";
                    
                    await Toast.Make(message).Show();
                    // Reset form to allow adding more colors
                    ResetColor();
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save color: {ex.Message}", "OK");
            }
        }

        private async Task HandleBackPress()
        {
            // Show a popup asking the user what they want to do
            bool discard = await Shell.Current.DisplayAlert(
                "Unsaved Changes",         // Title of the popup
                "Discard your current changes?", // Message
                "Discard",                 // Button 1
                "Cancel"                     // Button 2
            );

            // If the user chooses "Discard"
            if (discard)
            {
                // Go back to the previous page
                await Shell.Current.GoToAsync("..");
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
            
            // Sync text fields
            UpdateHexString();
            UpdateRgbString();
        }

        /// <summary>
        /// Converts current CMYK values to RGB.
        /// 
        /// CMYK TO RGB CONVERSION FORMULA:
        /// R = 255 × (1 - C) × (1 - K)
        /// G = 255 × (1 - M) × (1 - K)
        /// B = 255 × (1 - Y) × (1 - K)
        /// 
        /// Where C, M, Y, K are 0-1 values (we convert from 0-100 percentages)
        /// </summary>
        private void ConvertToRGB()
        {
            if (_isUpdatingFromRGB) return;  // Prevent circular updates

            _isUpdatingFromCMYK = true;  // Set guard

            // Convert from 0-100 to 0-1 range and clamp
            double c = Math.Clamp(CmykCyan / 100.0, 0, 1);
            double m = Math.Clamp(CmykMagenta / 100.0, 0, 1);
            double y = Math.Clamp(CmykYellow / 100.0, 0, 1);
            double k = Math.Clamp(CmykBlack / 100.0, 0, 1);

            // Apply conversion formula
            RgbRed = (int)Math.Round(255 * (1 - c) * (1 - k));
            RgbGreen = (int)Math.Round(255 * (1 - m) * (1 - k));
            RgbBlue = (int)Math.Round(255 * (1 - y) * (1 - k));

            _isUpdatingFromCMYK = false;  // Clear guard
            UpdateColor();  // Update preview
        }

        /// <summary>
        /// Converts current RGB values to CMYK.
        /// 
        /// RGB TO CMYK CONVERSION FORMULA:
        /// 1. Find K (black) = 1 - max(R, G, B)  [where R,G,B are 0-1]
        /// 2. C = (1 - R - K) / (1 - K)
        /// 3. M = (1 - G - K) / (1 - K)
        /// 4. Y = (1 - B - K) / (1 - K)
        /// 
        /// Special case: Pure black (R=G=B=0) gives K=100%, C=M=Y=0%
        /// </summary>
        private void UpdateCmykSlider()
        {
            if (_isUpdatingFromCMYK) return;  // Prevent circular updates

            _isUpdatingFromRGB = true;  // Set guard

            // Special case: Pure black
            if (RgbRed == 0 && RgbGreen == 0 && RgbBlue == 0)
            {
                CmykCyan = 0;
                CmykMagenta = 0;
                CmykYellow = 0;
                CmykBlack = 100;
                _isUpdatingFromRGB = false;
                return;
            }

            // Convert RGB from 0-255 to 0-1 range
            double r = RgbRed / 255.0;
            double g = RgbGreen / 255.0;
            double b = RgbBlue / 255.0;

            // Calculate K (black) - the darkest component
            double k = 1 - Math.Max(r, Math.Max(g, b));
            double invK = 1 - k;  // For division

            // Calculate C, M, Y and convert to 0-100 percentages
            CmykCyan = (int)Math.Round(100 * (1 - r - k) / invK);
            CmykMagenta = (int)Math.Round(100 * (1 - g - k) / invK);
            CmykYellow = (int)Math.Round(100 * (1 - b - k) / invK);
            CmykBlack = (int)Math.Round(100 * k);

            _isUpdatingFromRGB = false;  // Clear guard
        }

        private string _hexEntry;
        public string HexEntry
        {
            get => _hexEntry;
            set
            {
                if (_hexEntry == value) return;
                _hexEntry = value;
                OnPropertyChanged();
                if (!_isUpdatingFromSliders)
                    UpdateFromHex(value);
            }
        }

        private string _rgbEntry;
        public string RgbEntry
        {
            get => _rgbEntry;
            set
            {
                if (_rgbEntry == value) return;
                _rgbEntry = value;
                OnPropertyChanged();
                if (!_isUpdatingFromSliders)
                    UpdateFromRgbString(value);
            }
        }

        private bool _isUpdatingFromSliders;
        private bool _isUpdatingFromHex;

        private void UpdateFromHex(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return;

            // Allow typing partial hex without reset
            if (!hex.StartsWith("#")) return;
            if (hex.Length < 7) return; // Wait for full hex code

            try
            {
                var color = Color.FromArgb(hex);
                _isUpdatingFromCMYK = true; // Prevent circular updates
                _isUpdatingFromHex = true;
                
                RgbRed = (int)(color.Red * 255);
                RgbGreen = (int)(color.Green * 255);
                RgbBlue = (int)(color.Blue * 255);
                
                _isUpdatingFromHex = false;
                _isUpdatingFromCMYK = false;
                
                UpdateColor();
                UpdateCmykSlider();
                UpdateRgbString(); 
            }
            catch
            {
                // Invalid hex, ignore
            }
        }

        private void UpdateFromRgbString(string rgbString)
        {
            if (string.IsNullOrWhiteSpace(rgbString)) return;

            var parts = rgbString.Split(',');
            if (parts.Length != 3) return;

            if (int.TryParse(parts[0].Trim(), out int r) &&
                int.TryParse(parts[1].Trim(), out int g) &&
                int.TryParse(parts[2].Trim(), out int b))
            {
                _isUpdatingFromCMYK = true;
                RgbRed = Math.Clamp(r, 0, 255);
                RgbGreen = Math.Clamp(g, 0, 255);
                RgbBlue = Math.Clamp(b, 0, 255);
                _isUpdatingFromCMYK = false;

                UpdateColor();
                UpdateCmykSlider();
                UpdateHexString();
            }
        }

        private void UpdateHexString()
        {
            if (_isUpdatingFromHex) return;

            _isUpdatingFromSliders = true;
            HexEntry = $"#{RgbRed:X2}{RgbGreen:X2}{RgbBlue:X2}";
            _isUpdatingFromSliders = false;
        }

        private void UpdateRgbString()
        {
            _isUpdatingFromSliders = true;
            RgbEntry = $"{RgbRed},{RgbGreen},{RgbBlue}";
            _isUpdatingFromSliders = false;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
