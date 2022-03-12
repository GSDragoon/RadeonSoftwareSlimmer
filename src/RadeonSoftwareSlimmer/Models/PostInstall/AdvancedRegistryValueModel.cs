using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace RadeonSoftwareSlimmer.Models.PostInstall
{
    public class AdvancedRegistryValueModel : INotifyPropertyChanged
    {
        private object _currentValue;

        public AdvancedRegistryValueModel()
        {
            ReadOnly = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public string Description { get; set; }
        public string ValueName { get; set; }
        public RegistryValueKind RegistryValueType { get; set; }
        public object CurrentValue
        {
            get
            {
                return _currentValue;
            }
            set
            {
                _currentValue = value;

                if (_currentValue != null)
                {
                    Enabled = _currentValue.ToString().Equals(EnabledValue, StringComparison.OrdinalIgnoreCase);
                    OnPropertyChanged(nameof(Enabled));
                }
            }
        }
        public string EnabledValue { get; set; }
        public string DisabledValue { get; set; }
        public object DefaultValue { get; set; }
        public bool Enabled { get; set; }
        public bool ReadOnly { get; set; }

        public void Enable()
        {

        }

        public void Disable()
        {

        }

        public void ResetToDefault()
        {

        }
    }
}
