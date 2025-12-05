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
    /// <summary>
    /// ViewModel for the Create/Edit Palette page.
    /// 
    /// This ViewModel handles:
    /// - Creating and editing palettes (collections of colors)
    /// - Mixing colors together in specific ratios
    /// - Saving mix variants for later use
    /// - Matching colors to user-specified targets
    /// 
    /// Implements INotifyPropertyChanged: Notifies the View when data changes
    /// Implements IQueryAttributable: Receives navigation parameters (like PaletteId)
    /// </summary>
    public class CreatePaletteViewModel : INotifyPropertyChanged, IQueryAttributable
    {
        /// <summary>
        /// Event fired when any property changes.
        /// The View binds to properties and listens to this event to update the UI.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        // ===== SELECTION PROPERTIES =====
        // These track what the user has selected for deletion

        private Palette _selectedVariant;
        /// <summary>
        /// Currently selected color mix variant (for deletion).
        /// When set, clears SelectedPaletteColor (can only delete one thing at a time).
        /// </summary>
        public Palette SelectedVariant
        {
            get => _selectedVariant;
            set
            {
                _selectedVariant = value;
                OnPropertyChanged();
                if (_selectedVariant != null)
                {
                    SelectedPaletteColor = null;  // Deselect color if variant selected
                }
                OnPropertyChanged(nameof(IsDeleteVisible));  // Update delete button visibility
            }
        }

        private ColorEntity _selectedPaletteColor;
        /// <summary>
        /// Currently selected palette color (for deletion).
        /// When set, clears SelectedVariant (can only delete one thing at a time).
        /// </summary>
        public ColorEntity SelectedPaletteColor
        {
            get => _selectedPaletteColor;
            set
            {
                _selectedPaletteColor = value;
                OnPropertyChanged();
                if (_selectedPaletteColor != null)
                {
                    SelectedVariant = null;  // Deselect variant if color selected
                }
                OnPropertyChanged(nameof(IsDeleteVisible));  // Update delete button visibility
            }
        }

        /// <summary>
        /// Determines if the delete button should be visible.
        /// Shows when either a color or variant is selected.
        /// This is a computed property - it calculates the value from other properties.
        /// </summary>
        public bool IsDeleteVisible => SelectedPaletteColor != null || SelectedVariant != null;

        // ===== DATA COLLECTIONS AND STATE =====
        // These hold all the data needed by the View

        // Backing fields (private variables that store the actual data)
        private ObservableCollection<ColorEntity> _paletteColors = new();
        private ObservableCollection<MixColor> _mixComponents = new();
        private ObservableCollection<Palette> _variants = new();
        private Color _mixedColor = Colors.Gray;
        private string _blendMode = "RGB";
        private int? _currentPaletteId;  // null means creating new, number means editing existing
        private string _pageTitle = "Create Palette";
        private string _currentPaletteName = "";

        /// <summary>
        /// Colors in this palette.
        /// ObservableCollection automatically notifies the UI when items are added/removed.
        /// </summary>
        public ObservableCollection<ColorEntity> PaletteColors
        {
            get => _paletteColors;
            set { _paletteColors = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Components in the current color mix.
        /// Each component is a color with a specific ratio (e.g., "2 parts Red").
        /// </summary>
        public ObservableCollection<MixColor> MixComponents
        {
            get => _mixComponents;
            set { _mixComponents = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Saved color mix variants for this palette.
        /// Each variant is a specific combination of colors that produces a result color.
        /// </summary>
        public ObservableCollection<Palette> Variants
        {
            get => _variants;
            set { _variants = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// The resulting color from mixing the current components.
        /// Recalculated automatically when components or ratios change.
        /// </summary>
        public Color MixedColor
        {
            get => _mixedColor;
            set { _mixedColor = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Color blending mode ("RGB" is the only mode currently implemented).
        /// When changed, the mix is recalculated.
        /// </summary>
        public string BlendMode
        {
            get => _blendMode;
            set
            {
                if (_blendMode != value)
                {
                    _blendMode = value;
                    OnPropertyChanged();
                    RecalculateMix();  // Recalculate with new blend mode
                }
            }
        }

        /// <summary>
        /// Title shown at the top of the page.
        /// "Create Palette" when creating new, "Edit: [Name]" when editing.
        /// </summary>
        public string PageTitle
        {
            get => _pageTitle;
            set { _pageTitle = value; OnPropertyChanged(); }
        }

        // ===== DATABASE CONTEXT =====
        /// <summary>
        /// Database context for accessing color and palette data.
        /// Created directly here (not via DI) to keep ViewModel independent.
        /// </summary>
        private readonly ColorMixDbContext _dbContext;

        // ===== COMMANDS =====
        // Commands connect UI actions (button clicks) to ViewModel methods.
        // In XAML, you bind a Button's Command property to one of these.
        // When the button is clicked, the corresponding method runs.

        /// <summary>Opens dialog to add a color to the palette from the database.</summary>
        public ICommand AddColorCommand { get; }
        
        /// <summary>Removes the selected color or variant.</summary>
        public ICommand RemoveColorCommand { get; }
        
        /// <summary>Saves the current color mix as a named variant.</summary>
        public ICommand SaveVariantCommand { get; }
        
        /// <summary>Saves the entire palette to the database.</summary>
        public ICommand SavePaletteCommand { get; }
        
        /// <summary>Attempts to match a target color by adjusting mix ratios.</summary>
        public ICommand MatchCommand { get; }
        
        /// <summary>Calculates quantity estimates for mixing (e.g., how many liters of each color).</summary>
        public ICommand EstimateCommand { get; }
        
        /// <summary>Clears the current color mix.</summary>
        public ICommand ClearCommand { get; }
        
        /// <summary>Handles mode changes (used for toolbar actions).</summary>
        public ICommand ChangeModeCommand { get; }
        
        /// <summary>Increases the ratio of a component in the mix.</summary>
        public ICommand IncrementCommand { get; }
        
        /// <summary>Decreases the ratio of a component in the mix.</summary>
        public ICommand DecrementCommand { get; }
        
        /// <summary>Adds a palette color to the current mix.</summary>
        public ICommand AddColorToMixCommand { get; }
        
        /// <summary>Loads a saved variant into the current mix.</summary>
        public ICommand LoadVariantCommand { get; }

        public ICommand OnBackButtonPressedCommand { get; } // This command will be triggered when the user clicks BACK

        /// <summary>
        /// Constructor - Sets up the ViewModel when it's created.
        /// This is called automatically by MAUI when navigating to the page.
        /// </summary>
        public CreatePaletteViewModel()
        {
            // Set up database connection
            // Create options that specify SQLite database path
            var options = new DbContextOptionsBuilder<ColorMixDbContext>()
                .UseSqlite($"Filename={Path.Combine(FileSystem.AppDataDirectory, "colormix.db")}")
                .Options;
            _dbContext = new ColorMixDbContext(options);

            // Wire up commands to their handler methods
            // "new Command(OnAddColor)" means "when this command executes, call OnAddColor()"
            AddColorCommand = new Command(OnAddColor);
            RemoveColorCommand = new Command(OnRemoveColor);
            SaveVariantCommand = new Command(OnSaveVariant);
            SavePaletteCommand = new Command(OnSavePalette);
            MatchCommand = new Command(OnMatch);
            EstimateCommand = new Command(OnEstimate);
            ClearCommand = new Command(OnClear);
            ChangeModeCommand = new Command<string>(OnChangeMode);  // Takes a string parameter
            IncrementCommand = new Command<MixColor>(OnIncrement);  // Takes a MixColor parameter
            DecrementCommand = new Command<MixColor>(OnDecrement);
            AddColorToMixCommand = new Command<ColorEntity>(OnAddColorToMix);
            LoadVariantCommand = new Command<Palette>(OnLoadVariant);
            OnBackButtonPressedCommand = new Command(async () => await HandleBackPress());
            // Initialize database asynchronously
            InitializeAsync();
        }

        /// <summary>
        /// Initializes the database asynchronously.
        /// Called from constructor to ensure database is ready before use.
        /// Using "async void" here is okay because it's an event handler pattern.
        /// </summary>
        private async void InitializeAsync()
        {
            try
            {
                await _dbContext.InitializeDatabaseAsync();
            }
            catch (Exception ex)
            {
                // Log error but don't crash - database will be created on first use if this fails
                System.Diagnostics.Debug.WriteLine($"Error initializing CreatePaletteViewModel: {ex.Message}");
            }
        }

        /// <summary>
        /// Receives navigation parameters when navigating to this page.
        /// This is part of IQueryAttributable - MAUI calls this automatically.
        /// 
        /// Navigation example: Shell.GoToAsync($"CreatePalette?PaletteId={id}")
        /// The query dictionary would then contain { "PaletteId": id }
        /// </summary>
        /// <param name="query">Dictionary of navigation parameters</param>
        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // Check if we received a PaletteId parameter (means editing existing palette)
            if (query != null && query.ContainsKey("PaletteId") && query["PaletteId"] != null)
            {
                // Try to parse the ID as an integer
                if (int.TryParse(query["PaletteId"].ToString(), out int paletteId) && paletteId > 0)
                {
                    _currentPaletteId = paletteId;
                    await LoadPaletteAsync(paletteId);  // Load the existing palette
                    return;
                }
            }
            
            // No valid palette ID means we're creating a new palette
            _currentPaletteId = null;
            PageTitle = "Create Palette";
            _currentPaletteName = "";
            ResetState();  // Clear all data
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
                    // Update page title with palette name
                    _currentPaletteName = savedPalette.Name;
                    PageTitle = savedPalette.Name;

                    // Load colors
                    if (savedPalette.Colors != null)
                    {
                        foreach (var color in savedPalette.Colors)
                        {
                            try
                            {
                                var colorObj = Color.FromArgb(color.HexValue);
                                PaletteColors.Add(new ColorEntity
                                {
                                    ColorName = color.ColorName ?? "Unnamed",
                                    HexValue = color.HexValue ?? "#808080",
                                    Red = (int)(colorObj.Red * 255),
                                    Green = (int)(colorObj.Green * 255),
                                    Blue = (int)(colorObj.Blue * 255)
                                });
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error loading color {color.ColorName}: {ex.Message}");
                            }
                        }
                    }

                    // Load Variants for this palette
                    var variants = await _dbContext.PaletteVariants
                        .Where(v => v.SavedPaletteId == paletteId)
                        .Include(v => v.Components)
                        .ToListAsync();

                    if (variants != null)
                    {
                        foreach (var v in variants)
                        {
                            try
                            {
                                var p = new Palette(v.Name ?? "Variant", v.HexColor ?? "#808080", Color.FromArgb(v.HexColor ?? "#808080")) { Id = v.Id };
                                if (v.Components != null)
                                {
                                    foreach (var c in v.Components)
                                    {
                                        p.PaletteColors.Add(new MixColor(
                                            c.ColorName ?? "Color",
                                            Color.FromArgb(c.HexColor ?? "#808080"),
                                            c.Percentage,
                                            c.Ratio
                                        ));
                                    }
                                }
                                Variants.Add(p);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error loading variant {v.Name}: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading palette: {ex.Message}");
                await Toast.Make($"Error loading palette: {ex.Message}").Show();
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

        private void OnLoadVariant(Palette variant)
        {
            if (variant == null) return;

            try
            {
                // Clear existing mix components
                MixComponents.Clear();

                // Load variant's components into mix
                foreach (var color in variant.PaletteColors)
                {
                    MixComponents.Add(new MixColor(
                        color.ColorName,
                        color.Color,
                        color.Percentage,
                        color.Ratio
                    ));
                }

                // Set the mixed color
                MixedColor = variant.PaletteColor;

                Toast.Make($"Loaded variant: {variant.PaletteName}").Show();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading variant: {ex.Message}");
                Toast.Make("Error loading variant").Show();
            }
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

        /// <summary>
        /// Recalculates the mixed color based on current components and their ratios.
        /// 
        /// COLOR MIXING ALGORITHM:
        /// 1. Calculate each component's percentage of the total mix
        /// 2. For each RGB channel, calculate weighted average based on ratios
        /// 3. Normalize to 0-1 range (MAUI Color uses 0-1, not 0-255)
        /// 
        /// Example: 2 parts Red (#FF0000) + 1 part Blue (#0000FF)
        /// - Total ratio = 3
        /// - Red percentage = 66.67%, Blue = 33.33%
        /// - Final Red channel = (1.0 * 2 + 0.0 * 1) / 3 = 0.667
        /// - Final Green channel = (0.0 * 2 + 0.0 * 1) / 3 = 0.0
        /// - Final Blue channel = (0.0 * 2 + 1.0 * 1) / 3 = 0.333
        /// - Result = RGB(0.667, 0, 0.333) which is a purple color
        /// </summary>
        private void RecalculateMix()
        {
            // No components? Show gray as default
            if (!MixComponents.Any())
            {
                MixedColor = Colors.Gray;
                return;
            }

            // Calculate total ratio (sum of all component ratios)
            double totalRatio = MixComponents.Sum(c => c.Ratio);
            if (totalRatio == 0)
            {
                MixedColor = Colors.Gray;
                return;
            }

            // Variables to accumulate weighted RGB values
            double r = 0, g = 0, b = 0;

            // For each component, add its contribution to the final color
            foreach (var comp in MixComponents)
            {
                // Update percentage for UI display
                comp.Percentage = (comp.Ratio / totalRatio) * 100;
                
                // Add weighted RGB values
                // comp.Color.Red is 0-1, multiply by ratio to weight it
                r += comp.Color.Red * comp.Ratio;
                g += comp.Color.Green * comp.Ratio;
                b += comp.Color.Blue * comp.Ratio;
            }

            // Divide by total ratio to get average
            r /= totalRatio;
            g /= totalRatio;
            b /= totalRatio;

            // Clamp to valid range (0-1) in case of floating point errors
            r = Math.Clamp(r, 0, 1);
            g = Math.Clamp(g, 0, 1);
            b = Math.Clamp(b, 0, 1);

            // Create the final mixed color
            MixedColor = new Color((float)r, (float)g, (float)b);
        }

        /// <summary>
        /// Notifies the View that a property has changed.
        /// [CallerMemberName] automatically fills in the property name.
        /// The View's data bindings listen to PropertyChanged events and update the UI.
        /// </summary>
        /// <param name="propertyName">Name of the changed property (auto-filled)</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
