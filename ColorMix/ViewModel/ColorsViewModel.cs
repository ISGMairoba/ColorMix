/// <summary>
/// This file contains the ViewModel for the Colors list page.
/// This ViewModel manages:
/// - Displaying a list of saved colors
/// - Search and filtering
/// - Sorting by different criteria
/// - Single and batch operations (edit, duplicate, delete, share)
/// - Selection mode for multi-select
/// </summary>
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ColorMix.Data.Entities;
using ColorMix.Services;
using ColorMix.Models;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace ColorMix.ViewModel
{
    /// <summary>
    /// ViewModel for the Colors list page.
    /// Handles displaying colors with search, sort, and batch operation capabilities.
    /// </summary>
    public class ColorsViewModel : INotifyPropertyChanged
    {
        // Service for database operations
        private readonly IColorService _colorService;
        
        // Data collections
        private ObservableCollection<ColorsModel> _colorList = new();
        private List<ColorsModel> _allColors = new(); // Cache all colors for fast filtering
        
        // Search and sort state
        private string _searchText = string.Empty;
        private SortOption _currentSortOption = SortOption.DateCreated;
        private System.Threading.Timer _searchDebounceTimer;

        /// <summary>
        /// The filtered and sorted list of colors displayed in the UI.
        /// ObservableCollection automatically updates the UI when items change.
        /// </summary>
        public ObservableCollection<ColorsModel> ColorList
        {
            get => _colorList;
            set
            {
                _colorList = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Search text entered by the user.
        /// Uses debouncing: waits 750ms after user stops typing before searching.
        /// This prevents excessive filtering while user is still typing.
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged();
                
                // Debounce search to improve performance
                // Cancel any existing timer and start a new one
                _searchDebounceTimer?.Dispose();
                _searchDebounceTimer = new System.Threading.Timer(
                    _ => MainThread.BeginInvokeOnMainThread(() => ApplySortAndFilter()),
                    null,
                    750, // Wait 750ms after last keystroke
                    System.Threading.Timeout.Infinite  // Don't repeat
                );
            }
        }

        /// <summary>
        /// Currently selected sort option.
        /// When changed, immediately re-sorts and re-filters the list.
        /// </summary>
        public SortOption CurrentSortOption
        {
            get => _currentSortOption;
            set
            {
                if (_currentSortOption == value) return;
                _currentSortOption = value;
                OnPropertyChanged();
                ApplySortAndFilter();  // Immediately apply new sort
            }
        }

        /// <summary>
        /// List of all available sort options for binding to a picker/dropdown.
        /// Gets all values from the SortOption enum.
        /// </summary>
        public List<SortOption> SortOptions { get; } = Enum.GetValues(typeof(SortOption)).Cast<SortOption>().ToList();

        private bool _isSearching;
        /// <summary>
        /// Whether the search bar is currently visible.
        /// When set to false, clears the search text.
        /// </summary>
        public bool IsSearching
        {
            get => _isSearching;
            set
            {
                if (_isSearching == value) return;
                _isSearching = value;
                OnPropertyChanged();
                if (!_isSearching)
                {
                    SearchText = string.Empty; // Clear search when closing
                }
            }
        }

        private bool _isSelectionMode;
        /// <summary>
        /// Whether the UI is in selection mode for batch operations.
        /// When enabled, shows checkboxes and batch operation buttons.
        /// When disabled, clears all selections.
        /// </summary>
        public bool IsSelectionMode
        {
            get => _isSelectionMode;
            set
            {
                if (_isSelectionMode == value) return;
                _isSelectionMode = value;
                OnPropertyChanged();
                if (!_isSelectionMode)
                {
                    // Clear all selections when exiting selection mode
                    foreach (var color in ColorList) color.IsSelected = false;
                    OnPropertyChanged(nameof(SelectedCount));
                }
            }
        }


        /// <summary>
        /// Number of currently selected colors.
        /// Used to show "3 selected" in the UI.
        /// </summary>
        public int SelectedCount => ColorList.Count(c => c.IsSelected);

        // ===== COMMANDS =====
        // Single item operations
        public ICommand EditCommand { get; }
        public ICommand DuplicateCommand { get; }
        public ICommand ShareCommand { get; }
        public ICommand DeleteCommand { get; }
        
        // Batch operations
        public ICommand ToggleSelectionModeCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand DeselectAllCommand { get; }
        public ICommand BatchDeleteCommand { get; }
        public ICommand BatchDuplicateCommand { get; }
        public ICommand BatchShareCommand { get; }

        // UI controls
        public ICommand ToggleSearchCommand { get; }
        public ICommand OpenSortCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Constructor - receives ColorService via dependency injection.
        /// Sets up all commands and their handlers.
        /// </summary>
        /// <param name="colorService">Service for color data operations</param>
        public ColorsViewModel(IColorService colorService)
        {
            _colorService = colorService;

            // Wire up single-item commands
            EditCommand = new Command<ColorsModel>(async (color) => await OnEditAsync(color));
            DuplicateCommand = new Command<ColorsModel>(async (color) => await OnDuplicateAsync(color));
            ShareCommand = new Command<ColorsModel>(async (color) => await OnShareAsync(color));
            DeleteCommand = new Command<ColorsModel>(async (color) => await OnDeleteAsync(color));

            // Wire up selection mode commands
            ToggleSelectionModeCommand = new Command(() => IsSelectionMode = !IsSelectionMode);
            
            // Smart select all: toggles between all selected and none selected
            SelectAllCommand = new Command(() =>
            {
                // If all are selected, deselect all; otherwise select all
                bool allSelected = ColorList.All(c => c.IsSelected);
                foreach (var color in ColorList) color.IsSelected = !allSelected;
                OnPropertyChanged(nameof(SelectedCount));
            });

            DeselectAllCommand = new Command(() =>
            {
                foreach (var color in ColorList) color.IsSelected = false;
                OnPropertyChanged(nameof(SelectedCount));
            });

            // Wire up batch operation commands
            BatchDeleteCommand = new Command(async () => await OnBatchDeleteAsync());
            BatchDuplicateCommand = new Command(async () => await OnBatchDuplicateAsync());
            BatchShareCommand = new Command(async () => await OnBatchShareAsync());

            // Wire up UI control commands
            ToggleSearchCommand = new Command(() => IsSearching = !IsSearching);
            OpenSortCommand = new Command(async () => await OnOpenSortAsync());
        }

        public async Task LoadColorsAsync()
        {
            try
            {
                var colors = await _colorService.GetAllColorsAsync();
                _allColors.Clear();

                foreach (var colorEntity in colors)
                {
                    var colorModel = CreateColorModel(colorEntity);
                    _allColors.Add(colorModel);
                }
                
                ApplySortAndFilter();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load colors: {ex.Message}", "OK");
            }
        }

        private async Task OnBatchDeleteAsync()
        {
            var selectedColors = ColorList.Where(c => c.IsSelected).ToList();
            if (!selectedColors.Any()) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert("Delete Colors", 
                $"Are you sure you want to delete {selectedColors.Count} colors?", "Yes", "No");
            
            if (confirm)
            {
                var idsToDelete = selectedColors.Select(c => c.Id).ToList();
                await _colorService.DeleteColorsAsync(idsToDelete);

                // Remove from local lists without full reload
                foreach (var color in selectedColors)
                {
                    _allColors.Remove(color);
                    ColorList.Remove(color);
                }
                IsSelectionMode = false;
            }
        }

        private async Task OnOpenSortAsync()
        {
            string action = await Application.Current.MainPage.DisplayActionSheet("Sort By", "Cancel", null, 
                "Name", "Date Created", "Red", "Green", "Blue", "Hue");

            if (action == "Cancel" || action == null) return;

            CurrentSortOption = action switch
            {
                "Name" => SortOption.Name,
                "Date Created" => SortOption.DateCreated,
                "Red" => SortOption.Red,
                "Green" => SortOption.Green,
                "Blue" => SortOption.Blue,
                "Hue" => SortOption.Hue,
                _ => CurrentSortOption
            };
        }

        private async Task OnBatchDuplicateAsync()
        {
            var selectedColors = ColorList.Where(c => c.IsSelected).ToList();
            if (!selectedColors.Any()) return;

            var newColors = new List<ColorEntity>();
            foreach (var color in selectedColors)
            {
                newColors.Add(new ColorEntity
                {
                    ColorName = $"{color.ColorName} (Copy)",
                    HexValue = color.HexValue,
                    Red = (int)(color.Color.Red * 255),
                    Green = (int)(color.Color.Green * 255),
                    Blue = (int)(color.Color.Blue * 255)
                });
            }

            await _colorService.AddColorsAsync(newColors);
            
            // Local update: Add new colors to lists
            foreach (var entity in newColors)
            {
                var model = CreateColorModel(entity);
                _allColors.Add(model);
                ColorList.Add(model);
            }

            IsSelectionMode = false;
            // Removed await LoadColorsAsync();
        }

        private async Task OnBatchShareAsync()
        {
            var selectedColors = ColorList.Where(c => c.IsSelected).ToList();
            if (!selectedColors.Any()) return;

            var shareText = string.Join("\n\n", selectedColors.Select(c => 
                $"Color: {c.ColorName}\n" +
                $"Hex: {c.HexValue}\n" +
                $"RGB: {(int)(c.Color.Red * 255)}, {(int)(c.Color.Green * 255)}, {(int)(c.Color.Blue * 255)}"));

            await Share.RequestAsync(new ShareTextRequest
            {
                Text = shareText,
                Title = $"Share {selectedColors.Count} Colors"
            });
            
            IsSelectionMode = false;
        }

        /// <summary>
        /// Applies current sort option and search filter to the color list.
        /// This method is called when search text changes or sort option changes.
        /// </summary>
        private void ApplySortAndFilter()
        {
            IEnumerable<ColorsModel> filtered = _allColors;

            // Apply search filter if search text exists
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                // Search in both name and hex value (case-insensitive)
                filtered = filtered.Where(c => c.ColorName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || 
                                               c.HexValue.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Apply sorting based on current sort option
            filtered = CurrentSortOption switch
            {
                SortOption.Name => filtered.OrderBy(c => c.ColorName),
                SortOption.DateCreated => filtered.OrderByDescending(c => c.DateCreated),
                SortOption.Red => filtered.OrderByDescending(c => c.Color.Red),
                SortOption.Green => filtered.OrderByDescending(c => c.Color.Green),
                SortOption.Blue => filtered.OrderByDescending(c => c.Color.Blue),
                SortOption.Hue => filtered.OrderBy(c => c.Color.GetHue()),  // Order by hue (color wheel position)
                _ => filtered  // No sort
            };

            // Update the displayed list
            ColorList = new ObservableCollection<ColorsModel>(filtered);
        }

        public void ResetState()
        {
            IsSelectionMode = false;
            IsSearching = false;
            SearchText = string.Empty;
        }

        private async Task OnEditAsync(ColorsModel colorModel)
        {
            try
            {
                if (colorModel == null) return;

                var navigationParameter = new Dictionary<string, object>
                {
                    { "ColorId", colorModel.Id },
                    { "ColorName", colorModel.ColorName },
                    { "Red", (int)(colorModel.Color.Red * 255) },
                    { "Green", (int)(colorModel.Color.Green * 255) },
                    { "Blue", (int)(colorModel.Color.Blue * 255) },
                    { "HexValue", colorModel.HexValue }
                };

                // Use relative routing to preserve navigation stack
                await Shell.Current.GoToAsync("EditColor", navigationParameter);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to edit color: {ex.Message}", "OK");
            }
        }

        private async Task OnDuplicateAsync(ColorsModel colorModel)
        {
            try
            {
                if (colorModel == null) return;

                var newColor = new ColorEntity
                {
                    ColorName = $"{colorModel.ColorName} (Copy)",
                    HexValue = colorModel.HexValue,
                    Red = (int)(colorModel.Color.Red * 255),
                    Green = (int)(colorModel.Color.Green * 255),
                    Blue = (int)(colorModel.Color.Blue * 255)
                };

                await _colorService.AddColorAsync(newColor);
                
                // Local update
                var model = CreateColorModel(newColor);
                _allColors.Add(model);
                ColorList.Add(model);

                await Toast.Make($"Duplicated '{colorModel.ColorName}'").Show();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to duplicate color: {ex.Message}", "OK");
            }
        }

        private async Task OnShareAsync(ColorsModel colorModel)
        {
            try
            {
                if (colorModel == null) return;

                var shareText = $"Color: {colorModel.ColorName}\n" +
                                $"Hex: {colorModel.HexValue}\n" +
                                $"RGB: {(int)(colorModel.Color.Red * 255)}, {(int)(colorModel.Color.Green * 255)}, {(int)(colorModel.Color.Blue * 255)}";

                await Share.RequestAsync(new ShareTextRequest
                {
                    Text = shareText,
                    Title = $"Share {colorModel.ColorName}"
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to share color: {ex.Message}", "OK");
            }
        }

        private async Task OnDeleteAsync(ColorsModel colorModel)
        {
            try
            {
                if (colorModel == null) return;

                bool confirm = await Application.Current.MainPage.DisplayAlert(
                    "Delete Color",
                    $"Are you sure you want to delete '{colorModel.ColorName}'?",
                    "Delete",
                    "Cancel");

                if (confirm)
                {
                    await _colorService.DeleteColorAsync(colorModel.Id);
                    
                    // Local update
                    _allColors.Remove(colorModel);
                    ColorList.Remove(colorModel);

                    await Toast.Make($"Deleted '{colorModel.ColorName}'").Show();
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete color: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Creates a ColorsModel from a ColorEntity.
        /// Also hooks up property change events to update SelectionCount.
        /// </summary>
        /// <param name="colorEntity">Entity from database</param>
        /// <returns>Model for UI binding</returns>
        private ColorsModel CreateColorModel(ColorEntity colorEntity)
        {
            var color = Color.FromRgb(colorEntity.Red, colorEntity.Green, colorEntity.Blue);
            var colorModel = new ColorsModel(color, colorEntity.ColorName, colorEntity.HexValue)
            {
                Id = colorEntity.Id,
                DateCreated = colorEntity.CreatedAt
            };
            // Hook up property changed: when IsSelected changes, update SelectedCount
            colorModel.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == nameof(ColorsModel.IsSelected))
                    OnPropertyChanged(nameof(SelectedCount));
            };
            return colorModel;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
