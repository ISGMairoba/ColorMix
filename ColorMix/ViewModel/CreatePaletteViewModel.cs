using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ColorMix.Data;
using ColorMix.Data.Entities;
using ColorMix.Models;
using CommunityToolkit.Maui.Alerts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Graphics;

namespace ColorMix.ViewModel
{
    public class CreatePaletteViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Palette _selectedVariant;
        public Palette SelectedVariant
        {
            get => _selectedVariant;
            set
            {
                _selectedVariant = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ColorEntity> _paletteColors = new();
        private ObservableCollection<MixColor> _mixComponents = new();
        private ObservableCollection<Palette> _variants = new();
        private Color _mixedColor = Colors.Gray;
        private string _blendMode = "RGB";

        public ObservableCollection<ColorEntity> PaletteColors
        {
            get => _paletteColors;
            set { _paletteColors = value; OnPropertyChanged(); }
        }

        public ObservableCollection<MixColor> MixComponents
        {
            get => _mixComponents;
            set { _mixComponents = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Palette> Variants
        {
            get => _variants;
            set { _variants = value; OnPropertyChanged(); }
        }

        public Color MixedColor
        {
            get => _mixedColor;
            set { _mixedColor = value; OnPropertyChanged(); }
        }

        public string BlendMode
        {
            get => _blendMode;
            set
            {
                if (_blendMode != value)
                {
                    _blendMode = value;
                    OnPropertyChanged();
                    RecalculateMix();
                }
            }
        }

        private readonly ColorMixDbContext _dbContext;

        public ICommand AddColorCommand { get; }
        public ICommand RemoveColorCommand { get; }
        public ICommand SaveVariantCommand { get; }
        public ICommand SavePaletteCommand { get; }
        public ICommand MatchCommand { get; }
        public ICommand EstimateCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand ChangeModeCommand { get; }
        public ICommand IncrementCommand { get; }
        public ICommand DecrementCommand { get; }
        public ICommand AddColorToMixCommand { get; }

        public CreatePaletteViewModel()
        {
            // Manual injection for now as we don't have full DI setup in this snippet context
            // In a real app, this should be injected via constructor
            var options = new DbContextOptionsBuilder<ColorMixDbContext>()
                .UseSqlite($"Filename={Path.Combine(FileSystem.AppDataDirectory, "colormix.db")}")
                .Options;
            _dbContext = new ColorMixDbContext(options);

            AddColorCommand = new Command(OnAddColor);
            RemoveColorCommand = new Command(OnRemoveColor);
            SaveVariantCommand = new Command(OnSaveVariant);
            SavePaletteCommand = new Command(OnSavePalette);
            MatchCommand = new Command(OnMatch);
            EstimateCommand = new Command(OnEstimate);
            ClearCommand = new Command(OnClear);
            ChangeModeCommand = new Command<string>(OnChangeMode);
            IncrementCommand = new Command<MixColor>(OnIncrement);
            DecrementCommand = new Command<MixColor>(OnDecrement);
            AddColorToMixCommand = new Command<ColorEntity>(OnAddColorToMix);

            // Initialize async without blocking
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                await _dbContext.InitializeDatabaseAsync();
                await LoadColorsAsync();
                await LoadVariantsAsync();
            }
            catch (Exception ex)
            {
                // Log error but don't crash
                System.Diagnostics.Debug.WriteLine($"Error initializing CreatePaletteViewModel: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private async Task LoadColorsAsync()
        {
            PaletteColors.Clear();
            var colors = await _dbContext.Colors.ToListAsync();
            foreach (var color in colors)
            {
                PaletteColors.Add(color);
            }
            // No dummy data - user builds palette from scratch
        }

        private async Task LoadVariantsAsync()
        {
            Variants.Clear();
            var variants = await _dbContext.PaletteVariants.Include(v => v.Components).ToListAsync();
            foreach (var v in variants)
            {
                var p = new Palette(v.Name, v.HexColor, Color.FromArgb(v.HexColor));
                foreach (var c in v.Components)
                {
                    p.PaletteColors.Add(new MixColor(c.ColorName, Color.FromArgb(c.HexColor), c.Percentage, c.Ratio));
                }
                Variants.Add(p);
            }
        }



        private async void OnAddColor()
        {
            try
            {
                // Get all colors from database
                var allColors = await _dbContext.Colors.ToListAsync();
                
                if (!allColors.Any())
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("No Colors", "No colors available in database. Please add colors first.", "OK");
                    });
                    return;
                }

                // Show action sheet with color names
                var colorNames = allColors.Select(c => c.ColorName).ToArray();
                string selectedColorName = await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    return await Application.Current.MainPage.DisplayActionSheet("Select Color to Add", "Cancel", null, colorNames);
                });

                if (string.IsNullOrEmpty(selectedColorName) || selectedColorName == "Cancel")
                    return;

                // Find the selected color
                var selectedColor = allColors.FirstOrDefault(c => c.ColorName == selectedColorName);
                if (selectedColor != null && !PaletteColors.Any(p => p.ColorName == selectedColor.ColorName))
                {
                    PaletteColors.Add(selectedColor);
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Toast.Make($"Added {selectedColor.ColorName} to palette").Show();
                    });
                }
                else if (selectedColor != null)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("Already Added", "This color is already in the palette.", "OK");
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding color: {ex.Message}");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Failed to add color: {ex.Message}", "OK");
                });
            }
        }

        private void OnRemoveColor()
        {
            if (PaletteColors.Any())
            {
                var colorToRemove = PaletteColors.Last();
                PaletteColors.Remove(colorToRemove);
                
                // Also remove from mix if present
                var mixItem = MixComponents.FirstOrDefault(m => m.ColorName == colorToRemove.ColorName);
                if (mixItem != null)
                {
                    MixComponents.Remove(mixItem);
                    RecalculateMix();
                }
            }
        }

        private void OnAddColorToMix(ColorEntity color)
        {
            if (color == null) return;

            var existing = MixComponents.FirstOrDefault(c => c.ColorName == color.ColorName);
            if (existing != null)
            {
                existing.Ratio++;
            }
            else
            {
                MixComponents.Add(new MixColor(
                    color.ColorName,
                    Color.FromRgb(color.Red, color.Green, color.Blue),
                    0, // Percentage calculated later
                    1.0
                ));
            }
            RecalculateMix();
        }

        private void OnIncrement(MixColor item)
        {
            if (item != null)
            {
                item.Ratio++;
                RecalculateMix();
            }
        }

        private void OnDecrement(MixColor item)
        {
            if (item != null)
            {
                if (item.Ratio > 0)
                {
                    item.Ratio--;
                    if (item.Ratio == 0)
                    {
                        MixComponents.Remove(item);
                    }
                    RecalculateMix();
                }
            }
        }

        private async void OnSaveVariant()
        {
            if (!MixComponents.Any())
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No colors in mix to save", "OK");
                });
                return;
            }

            try
            {
                string name = await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    return await Application.Current.MainPage.DisplayPromptAsync("Save Variant", "Enter Variant Name:");
                });
                
                if (string.IsNullOrWhiteSpace(name)) return;

                var variantEntity = new PaletteVariantEntity
                {
                    Name = name,
                    HexColor = MixedColor.ToHex()
                };

            foreach (var comp in MixComponents)
            {
                variantEntity.Components.Add(new PaletteComponentEntity
                {
                    ColorName = comp.ColorName,
                    HexColor = comp.Color.ToHex(),
                    Ratio = comp.Ratio,
                    Percentage = comp.Percentage
                });
            }

            _dbContext.PaletteVariants.Add(variantEntity);
            await _dbContext.SaveChangesAsync();

            // Add to UI list
            var variant = new Palette(name, MixedColor.ToHex(), MixedColor);
            foreach (var comp in MixComponents)
            {
                variant.PaletteColors.Add(comp);
            }
            Variants.Add(variant);
            
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Toast.Make("Variant Saved Successfully").Show();
            });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving variant: {ex.Message}");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save variant: {ex.Message}", "OK");
                });
            }
        }

        private async void OnSavePalette()
        {
            try
            {
                string paletteName = await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    return await Application.Current.MainPage.DisplayPromptAsync("Save Palette", "Enter Palette Name:");
                });

                if (string.IsNullOrWhiteSpace(paletteName)) return;

                // TODO: Implement full palette saving logic
                // This will save:
                // 1. Palette name
                // 2. All colors in PaletteColors
                // 3. All variants with their components
                
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Info", "Save Palette functionality will be implemented to save the entire palette including all variants and colors.", "OK");
                    Toast.Make($"Palette '{paletteName}' save initiated").Show();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving palette: {ex.Message}");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save palette: {ex.Message}", "OK");
                });
            }
        }


        private async void OnMatch()
        {
            string result = await Application.Current.MainPage.DisplayPromptAsync("Match Color", "Enter Hex Color Code (e.g., #FF5733):", "Match", "Cancel", "#RRGGBB");
            if (string.IsNullOrWhiteSpace(result)) return;

            Color targetColor;
            try
            {
                targetColor = Color.FromArgb(result);
            }
            catch
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Invalid Hex Color Code", "OK");
                return;
            }

            await FindBestMix(targetColor);
        }

        private async Task FindBestMix(Color target)
        {
            if (!PaletteColors.Any())
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No colors in palette to mix with.", "OK");
                return;
            }

            // Simple Hill Climbing Algorithm
            // Initialize with equal ratios
            MixComponents.Clear();
            foreach (var color in PaletteColors)
            {
                MixComponents.Add(new MixColor(color.ColorName, Color.FromRgb(color.Red, color.Green, color.Blue), 0, 10));
            }
            RecalculateMix();

            double currentDiff = ColorDistance(MixedColor, target);
            bool improved = true;
            int maxIterations = 1000;
            int iteration = 0;

            while (improved && iteration < maxIterations)
            {
                improved = false;
                iteration++;

                foreach (var comp in MixComponents)
                {
                    // Try increasing
                    comp.Ratio++;
                    RecalculateMix();
                    double newDiff = ColorDistance(MixedColor, target);

                    if (newDiff < currentDiff)
                    {
                        currentDiff = newDiff;
                        improved = true;
                        continue;
                    }
                    else
                    {
                        // Revert
                        comp.Ratio--;
                    }

                    // Try decreasing
                    if (comp.Ratio > 0)
                    {
                        comp.Ratio--;
                        RecalculateMix();
                        newDiff = ColorDistance(MixedColor, target);

                        if (newDiff < currentDiff)
                        {
                            currentDiff = newDiff;
                            improved = true;
                            continue;
                        }
                        else
                        {
                            // Revert
                            comp.Ratio++;
                        }
                    }
                }
            }
            
            RecalculateMix(); // Ensure final state is consistent
            Toast.Make($"Matched with difference: {currentDiff:F2}").Show();
        }

        private double ColorDistance(Color c1, Color c2)
        {
            // Simple Euclidean distance in RGB
            return Math.Sqrt(
                Math.Pow(c1.Red - c2.Red, 2) +
                Math.Pow(c1.Green - c2.Green, 2) +
                Math.Pow(c1.Blue - c2.Blue, 2)
            );
        }

        private async void OnEstimate()
        {
            string result = await Application.Current.MainPage.DisplayPromptAsync("Estimate Quantity", "Enter Total Quantity (e.g., 10 Liters):", "Calculate", "Cancel", keyboard: Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(result) || !double.TryParse(result, out double totalQuantity))
            {
                if (result != null) // Only show error if not cancelled
                    await Application.Current.MainPage.DisplayAlert("Error", "Invalid Quantity", "OK");
                return;
            }

            double totalRatio = MixComponents.Sum(c => c.Ratio);
            if (totalRatio == 0) return;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Total Target: {totalQuantity}");
            sb.AppendLine("----------------");

            foreach (var comp in MixComponents)
            {
                double quantity = (comp.Ratio / totalRatio) * totalQuantity;
                sb.AppendLine($"{comp.ColorName}: {quantity:F2}");
            }

            await Application.Current.MainPage.DisplayAlert("Estimation Results", sb.ToString(), "OK");
        }

        private void OnClear()
        {
            MixComponents.Clear();
            MixedColor = Colors.Gray;
            Toast.Make("Mix Cleared").Show();
        }

        private void OnChangeMode(string parameter)
        {
             // Handle button clicks from the view if they use this command
             if (parameter == "SaveVariant") OnSaveVariant();
             else if (parameter == "Match") OnMatch();
             else if (parameter == "Estimate") OnEstimate();
             else if (parameter == "Clear") OnClear();
        }

        private void RecalculateMix()
        {
            if (!MixComponents.Any())
            {
                MixedColor = Colors.Gray;
                return;
            }

            double totalRatio = MixComponents.Sum(c => c.Ratio);
            if (totalRatio == 0)
            {
                MixedColor = Colors.Gray;
                return;
            }

            double r = 0, g = 0, b = 0;

            foreach (var comp in MixComponents)
            {
                comp.Percentage = (comp.Ratio / totalRatio) * 100;
                
                r += comp.Color.Red * comp.Ratio;
                g += comp.Color.Green * comp.Ratio;
                b += comp.Color.Blue * comp.Ratio;
            }

            r /= totalRatio;
            g /= totalRatio;
            b /= totalRatio;

            // Clamp values
            r = Math.Clamp(r, 0, 1);
            g = Math.Clamp(g, 0, 1);
            b = Math.Clamp(b, 0, 1);

            MixedColor = new Color((float)r, (float)g, (float)b);
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
