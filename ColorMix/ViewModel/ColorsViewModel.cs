using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ColorMix.Data.Entities;
using ColorMix.Services;
using ColorMix.Models;

namespace ColorMix.ViewModel
{
    public class ColorsViewModel : INotifyPropertyChanged
    {
        private readonly IColorService _colorService;
        private ObservableCollection<ColorsModel> _colorList = new();
        private List<ColorsModel> _allColors = new(); // Store all colors for filtering
        private string _searchText = string.Empty;
        private SortOption _currentSortOption = SortOption.DateCreated;

        public ObservableCollection<ColorsModel> ColorList
        {
            get => _colorList;
            set
            {
                _colorList = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged();
                ApplySortAndFilter();
            }
        }

        public SortOption CurrentSortOption
        {
            get => _currentSortOption;
            set
            {
                if (_currentSortOption == value) return;
                _currentSortOption = value;
                OnPropertyChanged();
                ApplySortAndFilter();
            }
        }

        public List<SortOption> SortOptions { get; } = Enum.GetValues(typeof(SortOption)).Cast<SortOption>().ToList();

        private bool _isSearching;
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
                    // Clear selection when exiting mode
                    foreach (var color in ColorList) color.IsSelected = false;
                    OnPropertyChanged(nameof(SelectedCount));
                }
            }
        }


        public int SelectedCount => ColorList.Count(c => c.IsSelected);

        public ICommand EditCommand { get; }
        public ICommand DuplicateCommand { get; }
        public ICommand ShareCommand { get; }
        public ICommand DeleteCommand { get; }
        
        // Batch Commands
        public ICommand ToggleSelectionModeCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand DeselectAllCommand { get; }
        public ICommand BatchDeleteCommand { get; }
        public ICommand BatchDuplicateCommand { get; }

        public ICommand ToggleSearchCommand { get; }
        public ICommand OpenSortCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ColorsViewModel(IColorService colorService)
        {
            _colorService = colorService;

            EditCommand = new Command<ColorsModel>(async (color) => await OnEditAsync(color));
            DuplicateCommand = new Command<ColorsModel>(async (color) => await OnDuplicateAsync(color));
            ShareCommand = new Command<ColorsModel>(async (color) => await OnShareAsync(color));
            DeleteCommand = new Command<ColorsModel>(async (color) => await OnDeleteAsync(color));

            ToggleSelectionModeCommand = new Command(() => IsSelectionMode = !IsSelectionMode);
            
            SelectAllCommand = new Command(() =>
            {
                foreach (var color in ColorList) color.IsSelected = true;
                OnPropertyChanged(nameof(SelectedCount));
            });

            DeselectAllCommand = new Command(() =>
            {
                foreach (var color in ColorList) color.IsSelected = false;
                OnPropertyChanged(nameof(SelectedCount));
            });

            BatchDeleteCommand = new Command(async () => await OnBatchDeleteAsync());
            BatchDuplicateCommand = new Command(async () => await OnBatchDuplicateAsync());

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
                    var color = Color.FromRgb(colorEntity.Red, colorEntity.Green, colorEntity.Blue);
                    var colorModel = new ColorsModel(color, colorEntity.ColorName, colorEntity.HexValue)
                    {
                        Id = colorEntity.Id,
                        DateCreated = colorEntity.CreatedAt
                    };
                    // Hook up property changed for selection count
                    colorModel.PropertyChanged += (s, e) => 
                    {
                        if (e.PropertyName == nameof(ColorsModel.IsSelected))
                            OnPropertyChanged(nameof(SelectedCount));
                    };
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
                foreach (var color in selectedColors)
                {
                    await _colorService.DeleteColorAsync(color.Id);
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

            foreach (var color in selectedColors)
            {
                await OnDuplicateAsync(color);
            }
            IsSelectionMode = false;
            await LoadColorsAsync(); // Reload to show duplicates
        }

        private void ApplySortAndFilter()
        {
            IEnumerable<ColorsModel> filtered = _allColors;

            // Filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(c => c.ColorName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || 
                                               c.HexValue.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Sort
            filtered = CurrentSortOption switch
            {
                SortOption.Name => filtered.OrderBy(c => c.ColorName),
                SortOption.DateCreated => filtered.OrderByDescending(c => c.DateCreated),
                SortOption.Red => filtered.OrderByDescending(c => c.Color.Red),
                SortOption.Green => filtered.OrderByDescending(c => c.Color.Green),
                SortOption.Blue => filtered.OrderByDescending(c => c.Color.Blue),
                SortOption.Hue => filtered.OrderBy(c => c.Color.GetHue()),
                _ => filtered
            };

            ColorList = new ObservableCollection<ColorsModel>(filtered);
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

                await Shell.Current.GoToAsync("//CreateColorsView", navigationParameter);
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
                await LoadColorsAsync();
                await Application.Current.MainPage.DisplayAlert("Success", $"Duplicated '{colorModel.ColorName}'", "OK");
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
                    await LoadColorsAsync();
                    await Application.Current.MainPage.DisplayAlert("Success", $"Deleted '{colorModel.ColorName}'", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete color: {ex.Message}", "OK");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
