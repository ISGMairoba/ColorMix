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
    public class CreatePaletteViewModel : INotifyPropertyChanged, IQueryAttributable
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
                if (_selectedVariant != null)
                {
                    SelectedPaletteColor = null;
                }
                OnPropertyChanged(nameof(IsDeleteVisible));
            }
        }

        private ColorEntity _selectedPaletteColor;
        public ColorEntity SelectedPaletteColor
        {
            get => _selectedPaletteColor;
            set
            {
                _selectedPaletteColor = value;
                OnPropertyChanged();
                if (_selectedPaletteColor != null)
                {
                    SelectedVariant = null;
                }
                OnPropertyChanged(nameof(IsDeleteVisible));
            }
        }

        public bool IsDeleteVisible => SelectedPaletteColor != null || SelectedVariant != null;

        private ObservableCollection<ColorEntity> _paletteColors = new();
        private ObservableCollection<MixColor> _mixComponents = new();
        private ObservableCollection<Palette> _variants = new();
        private Color _mixedColor = Colors.Gray;
        private string _blendMode = "RGB";
        private int? _currentPaletteId;

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

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                await _dbContext.InitializeDatabaseAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing CreatePaletteViewModel: {ex.Message}");
            }
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("PaletteId"))
            {
                if (int.TryParse(query["PaletteId"].ToString(), out int paletteId))
                {
                    _currentPaletteId = paletteId;
                    await LoadPaletteAsync(paletteId);
                }
            }
            else
            {
                _currentPaletteId = null;
                ResetState();
            }
        }

        private void ResetState()
        {
            PaletteColors.Clear();
            MixComponents.Clear();
            Variants.Clear();
            MixedColor = Colors.Gray;
            BlendMode = "RGB";
            SelectedPaletteColor = null;
            SelectedVariant = null;
        }

        private async Task LoadPaletteAsync(int paletteId)
        {
            try
            {
                ResetState();

                var savedPalette = await _dbContext.SavedPalettes
                    .Include(p => p.Colors)
                    .FirstOrDefaultAsync(p => p.Id == paletteId);

                if (savedPalette != null)
                {
                    foreach (var color in savedPalette.Colors)
                    {
                        // We need to find the original ColorEntity to get RGB values if possible, 
                        // or recreate it from Hex. SavedPaletteColorEntity has HexValue.
                        // Ideally we should link to ColorEntity, but for now we use the saved data.
                        // We need Red, Green, Blue for mixing.
                        var colorObj = Color.FromArgb(color.HexValue);
                        PaletteColors.Add(new ColorEntity
                        {
                            ColorName = color.ColorName,
                            HexValue = color.HexValue,
                            Red = (int)(colorObj.Red * 255),
                            Green = (int)(colorObj.Green * 255),
                            Blue = (int)(colorObj.Blue * 255)
                        });
                    }

                    // Load Variants for this palette
                    var variants = await _dbContext.PaletteVariants
                        .Where(v => v.SavedPaletteId == paletteId)
                        .Include(v => v.Components)
                        .ToListAsync();

                    foreach (var v in variants)
                    {
                        var p = new Palette(v.Name, v.HexColor, Color.FromArgb(v.HexColor)) { Id = v.Id };
                        foreach (var c in v.Components)
                        {
                            p.PaletteColors.Add(new MixColor(c.ColorName, Color.FromArgb(c.HexColor), c.Percentage, c.Ratio));
                        }
                        Variants.Add(p);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading palette: {ex.Message}");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to load palette data", "OK");
                });
            }
        }

        private async void OnAddColor()
        {
            try
            {
                var allColors = await _dbContext.Colors.ToListAsync();
                
                if (!allColors.Any())
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("No Colors", "No colors available in database. Please add colors first.", "OK");
                    });
                    return;
                }

                var popup = new Views.ColorSelectionPopup(allColors);
                await Application.Current.MainPage.Navigation.PushModalAsync(popup);
                
                // Wait for popup to close and check if color was selected
                await Task.Delay(100); // Small delay to ensure modal is shown
                
                // The popup will close itself when a color is selected
                // We need to wait for it to close and then check the result
                while (Application.Current.MainPage.Navigation.ModalStack.Contains(popup))
                {
                    await Task.Delay(100);
                }
                
                var selectedColor = popup.SelectedColor;
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

        private async void OnRemoveColor()
        {
            bool deleted = false;
            if (SelectedPaletteColor != null)
            {
                var colorToRemove = SelectedPaletteColor;
                PaletteColors.Remove(colorToRemove);
                
                var mixItem = MixComponents.FirstOrDefault(m => m.ColorName == colorToRemove.ColorName);
                if (mixItem != null)
                {
                    MixComponents.Remove(mixItem);
                    RecalculateMix();
                }
                SelectedPaletteColor = null;
                deleted = true;
                await MainThread.InvokeOnMainThreadAsync(() => Toast.Make($"Removed {colorToRemove.ColorName}").Show());
            }
            else if (SelectedVariant != null)
            {
                var variantToRemove = SelectedVariant;
                if (variantToRemove.Id > 0)
                {
                    var entity = await _dbContext.PaletteVariants.FindAsync(variantToRemove.Id);
                    if (entity != null)
                    {
                        _dbContext.PaletteVariants.Remove(entity);
                        await _dbContext.SaveChangesAsync();
                    }
                }
                Variants.Remove(variantToRemove);
                SelectedVariant = null;
                deleted = true;
                await MainThread.InvokeOnMainThreadAsync(() => Toast.Make($"Removed {variantToRemove.PaletteName}").Show());
            }
            else
            {
                await MainThread.InvokeOnMainThreadAsync(() => Toast.Make("Select a color or variant to delete").Show());
            }
            OnPropertyChanged(nameof(IsDeleteVisible));
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
                    0, 
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

                var variant = new Palette(name, MixedColor.ToHex(), MixedColor) { Id = variantEntity.Id };
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
            if (!PaletteColors.Any())
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No colors in palette to save", "OK");
                });
                return;
            }

            string name = await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                return await Application.Current.MainPage.DisplayPromptAsync("Save Palette", "Enter Palette Name:");
            });

            if (string.IsNullOrWhiteSpace(name)) return;

            try
            {
                var savedPalette = new SavedPaletteEntity
                {
                    Name = name,
                    DateCreated = DateTime.UtcNow
                };

                foreach (var color in PaletteColors)
                {
                    savedPalette.Colors.Add(new SavedPaletteColorEntity
                    {
                        ColorName = color.ColorName,
                        HexValue = color.HexValue
                    });
                }

                _dbContext.SavedPalettes.Add(savedPalette);
                await _dbContext.SaveChangesAsync();

                foreach (var variantModel in Variants)
                {
                    var variantEntity = await _dbContext.PaletteVariants.FindAsync(variantModel.Id);
                    if (variantEntity != null)
                    {
                        variantEntity.SavedPaletteId = savedPalette.Id;
                    }
                }
                await _dbContext.SaveChangesAsync();

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    Toast.Make("Palette Saved Successfully").Show();
                    // Navigate back to palettes page
                    await Shell.Current.GoToAsync("//MainPage");
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
                        comp.Ratio--;
                    }

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
                            comp.Ratio++;
                        }
                    }
                }
            }
            
            RecalculateMix();
            Toast.Make($"Matched with difference: {currentDiff:F2}").Show();
        }

        private double ColorDistance(Color c1, Color c2)
        {
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
                if (result != null)
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
