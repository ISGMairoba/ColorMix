/// <summary>
/// This file is the code-behind for MainPage.xaml - the main palette list page.
/// This is also a ViewModel (ContentPage + INotifyPropertyChanged).
/// 
/// This page combines View and ViewModel in one file (not pure MVVM, but simpler for this case).
/// It manages:
/// - Displaying saved palettes
/// - Search and filtering
/// - Sorting (by date or name)
/// - Selection mode for batch operations
/// - Edit, duplicate, share, delete operations (single and batch)
/// </summary>
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using ColorMix.Data;
using ColorMix.Data.Entities;
using ColorMix.Models;
using CommunityToolkit.Maui.Alerts;
using Microsoft.EntityFrameworkCore;

namespace ColorMix
{
    /// <summary>
    /// Main page showing the list of saved palettes.
    /// Implements INotifyPropertyChanged to support data binding (acts like a ViewModel).
    /// </summary>
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        public ObservableCollection<Palette> Palettes { get; set; } = new();
        private readonly ColorMixDbContext _dbContext;
        private System.Threading.Timer _searchDebounceTimer;

        private bool _isSearching;
        public bool IsSearching
        {
            get => _isSearching;
            set { _isSearching = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set 
            { 
                _searchText = value; 
                OnPropertyChanged();
                
                // Debounce search to improve performance
                _searchDebounceTimer?.Dispose();
                _searchDebounceTimer = new System.Threading.Timer(
                    _ => MainThread.BeginInvokeOnMainThread(async () => await LoadPalettesAsync()),
                    null,
                    750, // 750ms delay
                    System.Threading.Timeout.Infinite
                );
            }
        }

        private bool _isSelectionMode;
        public bool IsSelectionMode
        {
            get => _isSelectionMode;
            set { _isSelectionMode = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Palette> SelectedPalettes { get; set; } = new();

        public ICommand DuplicateCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ShareCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ToggleSearchCommand { get; }
        public ICommand OpenSortCommand { get; }
        public ICommand ToggleSelectionModeCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand DeselectAllCommand { get; }
        public ICommand BatchDeleteCommand { get; }
        public ICommand BatchDuplicateCommand { get; }
        public ICommand BatchShareCommand { get; }
        public ICommand ToggleSelectionCommand { get; }

        private string _currentSort = "Date"; // Date or Name

        public MainPage()
        {
            InitializeComponent();
            
            var options = new DbContextOptionsBuilder<ColorMixDbContext>()
                .UseSqlite($"Filename={Path.Combine(FileSystem.AppDataDirectory, "colormix.db")}")
                .Options;
            _dbContext = new ColorMixDbContext(options);

            DuplicateCommand = new Command<Palette>(OnDuplicate);
            EditCommand = new Command<Palette>(OnEdit);
            ShareCommand = new Command<Palette>(OnShare);
            DeleteCommand = new Command<Palette>(OnDelete);
            
            ToggleSearchCommand = new Command(() => 
            { 
                IsSearching = !IsSearching; 
                if (!IsSearching) SearchText = string.Empty; 
            });
            
            OpenSortCommand = new Command(OnOpenSort);
            ToggleSelectionModeCommand = new Command(() => 
            { 
                IsSelectionMode = !IsSelectionMode;
                SelectedPalettes.Clear();
                // Clear all IsSelected flags when exiting selection mode
                foreach(var p in Palettes) p.IsSelected = false;
            });

            SelectAllCommand = new Command(() =>
            {
                // Smart toggle: if all are selected, deselect all; otherwise select all
                bool allSelected = Palettes.All(p => p.IsSelected);
                
                SelectedPalettes.Clear();
                foreach (var p in Palettes)
                {
                    p.IsSelected = !allSelected;
                    if (!allSelected)
                    {
                        SelectedPalettes.Add(p);
                    }
                }
            });

            DeselectAllCommand = new Command(() => SelectedPalettes.Clear());
            
            BatchDeleteCommand = new Command(OnBatchDelete);
            BatchDuplicateCommand = new Command(OnBatchDuplicate);
            BatchShareCommand = new Command(OnBatchShare);
            ToggleSelectionCommand = new Command<Palette>(OnToggleSelection);

            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadPalettesAsync();
        }

        private async Task LoadPalettesAsync()
        {
            try
            {
                await _dbContext.InitializeDatabaseAsync();
                var query = _dbContext.SavedPalettes
                    .Include(p => p.Colors)
                    .AsQueryable();

                // Filter
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    query = query.Where(p => p.Name.ToLower().Contains(SearchText.ToLower()));
                }

                // Sort
                if (_currentSort == "Name")
                    query = query.OrderBy(p => p.Name);
                else
                    query = query.OrderByDescending(p => p.DateCreated);

                var savedPalettes = await query.ToListAsync();

                Palettes.Clear();
                foreach (var sp in savedPalettes)
                {
                    var palette = new Palette(sp.Name, "#000000", Colors.Black) { Id = sp.Id };
                    
                    // Load colors
                    if (sp.Colors != null)
                    {
                        foreach (var c in sp.Colors)
                        {
                            var colorObj = Color.FromArgb(c.HexValue);
                            palette.PaletteColors.Add(new MixColor(c.ColorName, colorObj, 0, 0));
                        }
                    }

                    // Load variants (need separate query as we can't include variants easily in the main query due to complexity or just to be safe)
                    var variants = await _dbContext.PaletteVariants
                        .Where(v => v.SavedPaletteId == sp.Id)
                        .Include(v => v.Components)
                        .ToListAsync();

                    foreach (var v in variants)
                    {
                        var p = new Palette(v.Name, v.HexColor, Color.FromArgb(v.HexColor)) { Id = v.Id };
                        palette.Variants.Add(p);
                    }

                    // Determine main color
                    if (palette.Variants.Any())
                    {
                        palette.PaletteColor = palette.Variants.First().PaletteColor;
                        palette.PaletteColorHex = palette.Variants.First().PaletteColorHex;
                    }
                    else if (palette.PaletteColors.Any())
                    {
                        palette.PaletteColor = palette.PaletteColors.First().Color;
                        palette.PaletteColorHex = palette.PaletteColors.First().Color.ToHex();
                    }
                    else
                    {
                        palette.PaletteColor = Colors.Gray;
                        palette.PaletteColorHex = "#808080";
                    }

                    Palettes.Add(palette);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load palettes: {ex.Message}", "OK");
            }
        }

        private void OnPaletteSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Palette selectedPalette)
            {
                if (IsSelectionMode)
                {
                    OnToggleSelection(selectedPalette);
                    // Clear selection so it doesn't stay highlighted by CollectionView default style
                    ((CollectionView)sender).SelectedItem = null;
                }
                else
                {
                    NavigateToEdit(selectedPalette);
                    ((CollectionView)sender).SelectedItem = null;
                }
            }
        }

        private async void OnEdit(Palette palette)
        {
            if (palette != null)
            {
                await NavigateToEdit(palette);
            }
        }

        private async Task NavigateToEdit(Palette palette)
        {
            try
            {
                var navigationParameter = new Dictionary<string, object>
                {
                    { "PaletteId", palette.Id }
                };
                await Shell.Current.GoToAsync(Constants.Routes.CreatePalette, navigationParameter);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                await DisplayAlert("Navigation Error", $"Failed to open palette editor: {ex.Message}", "OK");
            }
        }

        private async void OnDuplicate(Palette palette)
        {
            if (palette == null) return;

            try
            {
                var savedPalette = await _dbContext.SavedPalettes
                    .Include(p => p.Colors)
                    .FirstOrDefaultAsync(p => p.Id == palette.Id);

                if (savedPalette != null)
                {
                    var newPalette = new SavedPaletteEntity
                    {
                        Name = $"{savedPalette.Name} (Copy)",
                        DateCreated = DateTime.UtcNow
                    };

                    foreach (var c in savedPalette.Colors)
                    {
                        newPalette.Colors.Add(new SavedPaletteColorEntity
                        {
                            ColorName = c.ColorName,
                            HexValue = c.HexValue
                        });
                    }

                    _dbContext.SavedPalettes.Add(newPalette);
                    await _dbContext.SaveChangesAsync();

                    // Duplicate variants
                    var variants = await _dbContext.PaletteVariants
                        .Where(v => v.SavedPaletteId == palette.Id)
                        .Include(v => v.Components)
                        .ToListAsync();

                    foreach (var v in variants)
                    {
                        var newVariant = new PaletteVariantEntity
                        {
                            Name = v.Name,
                            HexColor = v.HexColor,
                            SavedPaletteId = newPalette.Id
                        };
                        
                        foreach(var c in v.Components)
                        {
                            newVariant.Components.Add(new PaletteComponentEntity
                            {
                                ColorName = c.ColorName,
                                HexColor = c.HexColor,
                                Ratio = c.Ratio,
                                Percentage = c.Percentage
                            });
                        }
                        _dbContext.PaletteVariants.Add(newVariant);
                    }
                    await _dbContext.SaveChangesAsync();

                    await Toast.Make("Palette Duplicated").Show();
                    await LoadPalettesAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to duplicate: {ex.Message}", "OK");
            }
        }

        private async void OnShare(Palette palette)
        {
            if (palette == null) return;

            try
            {
                // Create JSON export
                var jsonData = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Name = palette.PaletteName,
                    MainColor = palette.PaletteColorHex,
                    RGB = palette.DisplayText,
                    Colors = palette.PaletteColors.Select(c => new { c.ColorName, Hex = c.Color.ToHex() }).ToList(),
                    Variants = palette.Variants.Select(v => new { v.PaletteName, Hex = v.PaletteColorHex }).ToList()
                }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                // Create text export
                var textData = $"Palette: {palette.PaletteName}\n" +
                              $"Main Color: {palette.PaletteColorHex} ({palette.DisplayText})\n\n" +
                              $"Colors ({palette.PaletteColors.Count}):\n" +
                              string.Join("\n", palette.PaletteColors.Select(c => $"  - {c.ColorName}: {c.Color.ToHex()}")) +
                              $"\n\nVariants ({palette.Variants.Count}):\n" +
                              string.Join("\n", palette.Variants.Select(v => $"  - {v.PaletteName}: {v.PaletteColorHex}"));

                // Ask user which format
                var action = await DisplayActionSheet("Share Palette", "Cancel", null, "Share as JSON", "Share as Text", "Share Both");

                string shareContent = "";
                if (action == "Share as JSON")
                    shareContent = jsonData;
                else if (action == "Share as Text")
                    shareContent = textData;
                else if (action == "Share Both")
                    shareContent = $"=== JSON Format ===\n{jsonData}\n\n=== Text Format ===\n{textData}";
                else
                    return;

                await Share.Default.RequestAsync(new ShareTextRequest
                {
                    Title = $"Share Palette: {palette.PaletteName}",
                    Text = shareContent
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to share: {ex.Message}", "OK");
            }
        }

        private async void OnDelete(Palette palette)
        {
            if (palette == null) return;

            try
            {
                var confirm = await DisplayAlert(
                    "Delete Palette",
                    $"Are you sure you want to delete '{palette.PaletteName}'? This action cannot be undone.",
                    "Delete",
                    "Cancel");

                if (!confirm) return;

                await DeletePaletteInternal(palette);
                await Toast.Make($"Deleted '{palette.PaletteName}'").Show();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete: {ex.Message}", "OK");
            }
        }

        private async void OnOpenSort()
        {
            var action = await DisplayActionSheet("Sort By", "Cancel", null, "Date (Newest First)", "Name (A-Z)");
            if (action == "Date (Newest First)")
            {
                _currentSort = "Date";
                await LoadPalettesAsync();
            }
            else if (action == "Name (A-Z)")
            {
                _currentSort = "Name";
                await LoadPalettesAsync();
            }
        }

        private void OnToggleSelection(Palette palette)
        {
            if (palette == null) return;
            
            palette.IsSelected = !palette.IsSelected;
            if (palette.IsSelected)
                SelectedPalettes.Add(palette);
            else
                SelectedPalettes.Remove(palette);
        }

        private async void OnBatchDelete()
        {
            if (!SelectedPalettes.Any()) return;

            var confirm = await DisplayAlert("Delete Palettes", $"Delete {SelectedPalettes.Count} palettes?", "Delete", "Cancel");
            if (!confirm) return;

            foreach (var p in SelectedPalettes.ToList())
            {
                await DeletePaletteInternal(p);
            }
            SelectedPalettes.Clear();
            IsSelectionMode = false;
        }

        private async void OnBatchDuplicate()
        {
            if (!SelectedPalettes.Any()) return;

            var tasks = new List<Task>();
            foreach (var p in SelectedPalettes.ToList())
            {
                var savedPalette = await _dbContext.SavedPalettes
                    .Include(sp => sp.Colors)
                    .FirstOrDefaultAsync(sp => sp.Id == p.Id);

                if (savedPalette != null)
                {
                    var newPalette = new SavedPaletteEntity
                    {
                        Name = $"{savedPalette.Name} (Copy)",
                        DateCreated = DateTime.UtcNow
                    };

                    foreach (var c in savedPalette.Colors)
                    {
                        newPalette.Colors.Add(new SavedPaletteColorEntity
                        {
                            ColorName = c.ColorName,
                            HexValue = c.HexValue
                        });
                    }

                    _dbContext.SavedPalettes.Add(newPalette);
                    await _dbContext.SaveChangesAsync();

                    // Duplicate variants
                    var variants = await _dbContext.PaletteVariants
                        .Where(v => v.SavedPaletteId == p.Id)
                        .Include(v => v.Components)
                        .ToListAsync();

                    foreach (var v in variants)
                    {
                        var newVariant = new PaletteVariantEntity
                        {
                            Name = v.Name,
                            HexColor = v.HexColor,
                            SavedPaletteId = newPalette.Id
                        };
                        
                        foreach(var c in v.Components)
                        {
                            newVariant.Components.Add(new PaletteComponentEntity
                            {
                                ColorName = c.ColorName,
                                HexColor = c.HexColor,
                                Ratio = c.Ratio,
                                Percentage = c.Percentage
                            });
                        }
                        _dbContext.PaletteVariants.Add(newVariant);
                    }
                    await _dbContext.SaveChangesAsync();
                }
            }
            
            await LoadPalettesAsync();
            SelectedPalettes.Clear();
            IsSelectionMode = false;
            await Toast.Make("Palettes duplicated").Show();
        }

        private async void OnBatchShare()
        {
            if (!SelectedPalettes.Any()) return;
            
            // Share summary of selected
            var textData = "Shared Palettes:\n\n" + string.Join("\n\n", SelectedPalettes.Select(p => $"{p.PaletteName}: {p.PaletteColorHex}"));
            
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = "Shared Palettes",
                Text = textData
            });
            
            SelectedPalettes.Clear();
            IsSelectionMode = false;
        }

        private async Task DeletePaletteInternal(Palette palette)
        {
            try
            {
                var savedPalette = await _dbContext.SavedPalettes.FirstOrDefaultAsync(p => p.Id == palette.Id);
                if (savedPalette != null)
                {
                    var variants = await _dbContext.PaletteVariants.Where(v => v.SavedPaletteId == palette.Id).ToListAsync();
                    _dbContext.PaletteVariants.RemoveRange(variants);
                    _dbContext.SavedPalettes.Remove(savedPalette);
                    await _dbContext.SaveChangesAsync();
                    Palettes.Remove(palette);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting palette: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
