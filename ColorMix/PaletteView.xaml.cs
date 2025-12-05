using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using ColorMix.Data;
using ColorMix.Data.Entities;
using ColorMix.Models;
using ColorMix.ViewModels;
using CommunityToolkit.Maui.Alerts;
using Microsoft.EntityFrameworkCore;

namespace ColorMix
{
    /// <summary>
    /// Main page showing the list of saved palettes.
    /// Implements INotifyPropertyChanged to support data binding (acts like a ViewModel).
    /// </summary>
    public partial class PaletteView : ContentPage, INotifyPropertyChanged
    {
        private PalettesViewModel ViewModel => BindingContext as PalettesViewModel;

        public PaletteView()
        {
            InitializeComponent();
            BindingContext = new PalettesViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ViewModel.LoadPalettesAsync();
        }

        private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Palette selectedPalette)
            {
                if (ViewModel.IsSelectionMode)
                {
                    ViewModel.ToggleSelectionCommand.Execute(selectedPalette);
                    ((CollectionView)sender).SelectedItem = null;
                }
                else
                {
                    ViewModel.EditCommand.Execute(selectedPalette);
                    ((CollectionView)sender).SelectedItem = null;
                }
            }
        }
    }
}
