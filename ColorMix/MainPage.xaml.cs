using System.Collections.ObjectModel;
using System.Windows.Input;
using ColorMix.Data;
using ColorMix.Data.Entities;
using ColorMix.Models;
using CommunityToolkit.Maui.Alerts;
using Microsoft.EntityFrameworkCore;

namespace ColorMix
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Palette> Palettes { get; set; } = new();
        private readonly ColorMixDbContext _dbContext;

        public ICommand DuplicateCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ShareCommand { get; }

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
                var savedPalettes = await _dbContext.SavedPalettes
                    .Include(p => p.Colors)
                    .OrderByDescending(p => p.DateCreated)
                    .ToListAsync();

                Palettes.Clear();

                foreach (var saved in savedPalettes)
                {
                    // Load Variants for this palette
                    var variants = await _dbContext.PaletteVariants
                        .Where(v => v.SavedPaletteId == saved.Id)
                        .Include(v => v.Components)
                        .ToListAsync();

                    // Determine main color (e.g., first variant or first color)
                    var mainColorHex = variants.FirstOrDefault()?.HexColor ?? saved.Colors.FirstOrDefault()?.HexValue ?? "#808080";
                    var mainColor = Color.FromArgb(mainColorHex);

                    var palette = new Palette(saved.Name, mainColorHex, mainColor) { Id = saved.Id };

                    // Add Colors
                    foreach (var c in saved.Colors)
                    {
                        palette.PaletteColors.Add(new MixColor(c.ColorName, Color.FromArgb(c.HexValue), 0, 1));
                    }

                    // Add Variants
                    foreach (var v in variants)
                    {
                        var p = new Palette(v.Name, v.HexColor, Color.FromArgb(v.HexColor)) { Id = v.Id };
                        palette.Variants.Add(p);
                    }

                    Palettes.Add(palette);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading palettes: {ex.Message}");
            }
        }

        private async void OnPaletteSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Palette selectedPalette)
            {
                await NavigateToEdit(selectedPalette);
                ((CollectionView)sender).SelectedItem = null;
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
            var navigationParameter = new Dictionary<string, object>
            {
                { "PaletteId", palette.Id }
            };
            await Shell.Current.GoToAsync("CreatePaletteView", navigationParameter);
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
            await DisplayAlert("Share", $"Sharing {palette.PaletteName} functionality coming soon!", "OK");
        }
    }
}
