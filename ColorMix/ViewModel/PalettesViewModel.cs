using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using ColorMix.Data;
using ColorMix.Data.Entities;
using ColorMix.Models;
using CommunityToolkit.Maui.Alerts;
using Microsoft.EntityFrameworkCore;


namespace ColorMix.ViewModels
{
    public class PalettesViewModel : INotifyPropertyChanged
    {
        private readonly ColorMixDbContext _dbContext;

        public ObservableCollection<Palette> Palettes { get; } = new();
        public ObservableCollection<Palette> SelectedPalettes { get; } = new();

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                DebounceSearch();
            }
        }

        private bool _isSearching;
        public bool IsSearching
        {
            get => _isSearching;
            set { _isSearching = value; OnPropertyChanged(); }
        }

        private bool _isSelectionMode;
        public bool IsSelectionMode
        {
            get => _isSelectionMode;
            set
            {
                _isSelectionMode = value;
                OnPropertyChanged();

                // Clear selection when exiting selection mode
                if (!_isSelectionMode)
                    SelectedPalettes.Clear();
            }
        }

        private string _currentSort = "Date"; // Date or Name
        private System.Threading.Timer _searchDebounceTimer;

        // Commands
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

        public PalettesViewModel()
        {
            // Initialize DbContext
            var options = new DbContextOptionsBuilder<ColorMixDbContext>()
                .UseSqlite($"Filename={Path.Combine(FileSystem.AppDataDirectory, "colormix.db")}")
                .Options;

            _dbContext = new ColorMixDbContext(options);

            // Initialize commands
            DuplicateCommand = new Command<Palette>(async p => await DuplicatePaletteAsync(p));
            EditCommand = new Command<Palette>(async p => await EditPaletteAsync(p));
            ShareCommand = new Command<Palette>(async p => await SharePaletteAsync(p));
            DeleteCommand = new Command<Palette>(async p => await DeletePaletteAsync(p));

            ToggleSearchCommand = new Command(() =>
            {
                IsSearching = !IsSearching;
                if (!IsSearching) SearchText = string.Empty;
            });

            OpenSortCommand = new Command(async () => await OpenSortAsync());
            ToggleSelectionModeCommand = new Command(() =>
            {
                IsSelectionMode = !IsSelectionMode;
                foreach (var p in Palettes) p.IsSelected = false;
            });

            SelectAllCommand = new Command(() => SelectAllPalettes());
            DeselectAllCommand = new Command(() => SelectedPalettes.Clear());

            BatchDeleteCommand = new Command(async () => await BatchDeleteAsync());
            BatchDuplicateCommand = new Command(async () => await BatchDuplicateAsync());
            BatchShareCommand = new Command(async () => await BatchShareAsync());
            ToggleSelectionCommand = new Command<Palette>(ToggleSelection);
        }

        #region Public Methods

        public async Task LoadPalettesAsync()
        {
            try
            {
                await _dbContext.InitializeDatabaseAsync();

                var query = _dbContext.SavedPalettes.Include(p => p.Colors).AsQueryable();

                // Filter
                if (!string.IsNullOrWhiteSpace(SearchText))
                    query = query.Where(p => p.Name.ToLower().Contains(SearchText.ToLower()));

                // Sort
                query = _currentSort == "Name"
                    ? query.OrderBy(p => p.Name)
                    : query.OrderByDescending(p => p.DateCreated);

                var savedPalettes = await query.ToListAsync();

                Palettes.Clear();
                foreach (var sp in savedPalettes)
                {
                    var palette = new Palette(sp.Name, "#000000", Colors.Black) { Id = sp.Id };

                    if (sp.Colors != null)
                        foreach (var c in sp.Colors)
                            palette.PaletteColors.Add(new MixColor(c.ColorName, Color.FromArgb(c.HexValue), 0, 0));

                    // Load variants separately
                    var variants = await _dbContext.PaletteVariants
                        .Where(v => v.SavedPaletteId == sp.Id)
                        .Include(v => v.Components)
                        .ToListAsync();

                    foreach (var v in variants)
                        palette.Variants.Add(new Palette(v.Name, v.HexColor, Color.FromArgb(v.HexColor)) { Id = v.Id });

                    // Set main color
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
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to load palettes: {ex.Message}", "OK");
            }
        }

        #endregion

        #region Command Handlers

        private void DebounceSearch()
        {
            _searchDebounceTimer?.Dispose();
            _searchDebounceTimer = new System.Threading.Timer(
                _ => MainThread.BeginInvokeOnMainThread(async () => await LoadPalettesAsync()),
                null,
                750,
                System.Threading.Timeout.Infinite
            );
        }

        private void ToggleSelection(Palette palette)
        {
            if (palette == null) return;

            palette.IsSelected = !palette.IsSelected;
            if (palette.IsSelected) SelectedPalettes.Add(palette);
            else SelectedPalettes.Remove(palette);
        }

        private void SelectAllPalettes()
        {
            bool allSelected = Palettes.All(p => p.IsSelected);
            SelectedPalettes.Clear();

            foreach (var p in Palettes)
            {
                p.IsSelected = !allSelected;
                if (!allSelected) SelectedPalettes.Add(p);
            }
        }

        private async Task OpenSortAsync()
        {
            var action = await App.Current.MainPage.DisplayActionSheet("Sort By", "Cancel", null, "Date (Newest First)", "Name (A-Z)");
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

        #endregion

        #region Palette Operations

        private async Task EditPaletteAsync(Palette palette)
        {
            if (palette == null) return;
            var navParams = new Dictionary<string, object> { { "PaletteId", palette.Id } };
            await Shell.Current.GoToAsync(Constants.Routes.CreatePalette, navParams);
        }

        private async Task DuplicatePaletteAsync(Palette palette)
        {
            if (palette == null) return;

            try
            {
                var savedPalette = await _dbContext.SavedPalettes
                    .Include(p => p.Colors)
                    .FirstOrDefaultAsync(p => p.Id == palette.Id);

                if (savedPalette == null) return;

                var newPalette = new SavedPaletteEntity
                {
                    Name = $"{savedPalette.Name} (Copy)",
                    DateCreated = DateTime.UtcNow
                };

                foreach (var c in savedPalette.Colors)
                    newPalette.Colors.Add(new SavedPaletteColorEntity { ColorName = c.ColorName, HexValue = c.HexValue });

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

                    foreach (var c in v.Components)
                        newVariant.Components.Add(new PaletteComponentEntity
                        {
                            ColorName = c.ColorName,
                            HexColor = c.HexColor,
                            Ratio = c.Ratio,
                            Percentage = c.Percentage
                        });

                    _dbContext.PaletteVariants.Add(newVariant);
                }

                await _dbContext.SaveChangesAsync();
                await Toast.Make("Palette Duplicated").Show();
                await LoadPalettesAsync();
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to duplicate: {ex.Message}", "OK");
            }
        }

        private async Task DeletePaletteAsync(Palette palette)
        {
            if (palette == null) return;

            var confirm = await App.Current.MainPage.DisplayAlert(
                "Delete Palette",
                $"Are you sure you want to delete '{palette.PaletteName}'?",
                "Delete",
                "Cancel");

            if (!confirm) return;

            try
            {
                await DeletePaletteInternalAsync(palette);
                await Toast.Make($"Deleted '{palette.PaletteName}'").Show();
            }
            catch { }
        }

        private async Task DeletePaletteInternalAsync(Palette palette)
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

        private async Task BatchDeleteAsync()
        {
            if (!SelectedPalettes.Any()) return;

            var confirm = await App.Current.MainPage.DisplayAlert(
                "Delete Palettes",
                $"Delete {SelectedPalettes.Count} palettes?",
                "Delete",
                "Cancel");

            if (!confirm) return;

            foreach (var p in SelectedPalettes.ToList())
                await DeletePaletteInternalAsync(p);

            SelectedPalettes.Clear();
            IsSelectionMode = false;
        }

        private async Task BatchDuplicateAsync()
        {
            foreach (var p in SelectedPalettes.ToList())
                await DuplicatePaletteAsync(p);

            SelectedPalettes.Clear();
            IsSelectionMode = false;
            await Toast.Make("Palettes duplicated").Show();
        }

        private async Task BatchShareAsync()
        {
            if (!SelectedPalettes.Any()) return;

            var textData = "Shared Palettes:\n\n" + string.Join("\n\n",
                SelectedPalettes.Select(p => $"{p.PaletteName}: {p.PaletteColorHex}"));

            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = "Shared Palettes",
                Text = textData
            });

            SelectedPalettes.Clear();
            IsSelectionMode = false;
        }

        private async Task SharePaletteAsync(Palette palette)
        {
            if (palette == null) return;

            try
            {
                var jsonData = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Name = palette.PaletteName,
                    MainColor = palette.PaletteColorHex,
                    RGB = palette.DisplayText,
                    Colors = palette.PaletteColors.Select(c => new { c.ColorName, Hex = c.Color.ToHex() }),
                    Variants = palette.Variants.Select(v => new { v.PaletteName, Hex = v.PaletteColorHex })
                }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                var textData = $"Palette: {palette.PaletteName}\nMain Color: {palette.PaletteColorHex} ({palette.DisplayText})\n\n" +
                               $"Colors ({palette.PaletteColors.Count}):\n" +
                               string.Join("\n", palette.PaletteColors.Select(c => $"  - {c.ColorName}: {c.Color.ToHex()}")) +
                               $"\n\nVariants ({palette.Variants.Count}):\n" +
                               string.Join("\n", palette.Variants.Select(v => $"  - {v.PaletteName}: {v.PaletteColorHex}"));

                var action = await App.Current.MainPage.DisplayActionSheet("Share Palette", "Cancel", null, "Share as JSON", "Share as Text", "Share Both");

                string shareContent = action switch
                {
                    "Share as JSON" => jsonData,
                    "Share as Text" => textData,
                    "Share Both" => $"=== JSON ===\n{jsonData}\n\n=== Text ===\n{textData}",
                    _ => null
                };

                if (shareContent != null)
                    await Share.Default.RequestAsync(new ShareTextRequest
                    {
                        Title = $"Share Palette: {palette.PaletteName}",
                        Text = shareContent
                    });
            }
            catch { }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        #endregion
    }
}
