using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ColorMix.Models
{
    public class ColorsModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public Color Color { get; set; }
        public string ColorName { get; set; }
        public string HexValue { get; set; }
        public DateTime DateCreated { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public string DisplayText => $"rgb({Color.Red *255}, {Color.Green * 255},{Color.Blue * 255}) - {HexValue}";
        
        public ColorsModel(Color color, string colorName, string hexValue) 
        { 
            this.Color = color;
            this.ColorName = colorName;
            this.HexValue = hexValue;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
