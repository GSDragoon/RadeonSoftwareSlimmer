using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Win32;

namespace RadeonSoftwareSlimmer.Models.PostInstall
{
    public class AdvancedRegistryModel : INotifyPropertyChanged
    {
        private static readonly string ADVANCED_REGISTRY_KEY = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}";
        private static readonly string VALUE_IDENTIFIER_NAME = "ProviderName";
        private static readonly string VALUE_IDENTIFIER_VALUE = "Advanced Micro Devices, Inc.";

        private bool _AmdDisplayAdapterFound;
        private string _displayAdapterId;

        public AdvancedRegistryModel()
        {
            _AmdDisplayAdapterFound = false;
            _displayAdapterId = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public AdvancedRegistryValueModel DriverUpdateComponent { get; set; }
        public AdvancedRegistryValueModel AmdLinkContextMenu { get; set; }
        public AdvancedRegistryValueModel AmdLinkDevices { get; set; }
        public AdvancedRegistryValueModel ReleaseNotes { get; set; }

        public void LoadOrRefresh()
        {
            DriverUpdateComponent = new AdvancedRegistryValueModel()
            {
                Description = "Driver Update Component",
                ValueName = "driverupdate_ui_component_na",
                DefaultValue = "false",
                EnabledValue = "false",
                DisabledValue = "true"
            };
            AmdLinkContextMenu = new AdvancedRegistryValueModel()
            {
                Description = "AMD Link Desktop Context Menu",
                ValueName = "HideAMDLinkForWindows",
                DefaultValue = "0",
                EnabledValue = "0",
                DisabledValue = "1"
            };
            AmdLinkDevices = new AdvancedRegistryValueModel()
            {
                Description = "Devices (AMD Link)",
                ValueName = "mobile_runtime_component_NA",
                DefaultValue = "false",
                EnabledValue = "false",
                DisabledValue = "true"
            };
            ReleaseNotes = new AdvancedRegistryValueModel()
            {
                Description = "Release Notes",
                ValueName = "ShowReleaseNotes",
                DefaultValue = "1",
                EnabledValue = "1",
                DisabledValue = "0"
            };
            ReleaseNotes = new AdvancedRegistryValueModel()
            {
                Description = "Audio Driver Version",
                ValueName = "AudioDriverVersion_hide",
                RegistryValueType = RegistryValueKind.String,
                DefaultValue = "true",
                EnabledValue = "true",
                DisabledValue = "false"
            };

            LoadAmdRegistryKeyName();

            OnPropertyChanged(nameof(DriverUpdateComponent));
            OnPropertyChanged(nameof(AmdLinkContextMenu));
            OnPropertyChanged(nameof(AmdLinkDevices));
            OnPropertyChanged(nameof(ReleaseNotes));
        }

        public void Enable()
        {

        }

        public void Disable()
        {

        }

        public void ResetToDefault()
        {

        }

        private void LoadAmdRegistryKeyName()
        {
            using (RegistryKey displayAdaptersKey = Registry.LocalMachine.OpenSubKey(ADVANCED_REGISTRY_KEY))
            {
                foreach (string childKey in displayAdaptersKey.GetSubKeyNames())
                {
                    if (childKey.Equals("Configuration", StringComparison.OrdinalIgnoreCase) || childKey.Equals("Properties"))
                        continue;

                    using (RegistryKey displayAdapterkey = displayAdaptersKey.OpenSubKey(childKey))
                    {
                        object provider = displayAdapterkey.GetValue(VALUE_IDENTIFIER_NAME);
                        if (provider != null)
                        {
                            string strProvider = provider.ToString();
                            
                            if (strProvider.Equals(VALUE_IDENTIFIER_VALUE, StringComparison.OrdinalIgnoreCase))
                            {
                                _AmdDisplayAdapterFound = true;
                                _displayAdapterId = childKey;

                                DriverUpdateComponent.CurrentValue = displayAdapterkey.GetValue(DriverUpdateComponent.ValueName);
                                AmdLinkContextMenu.CurrentValue = displayAdapterkey.GetValue(AmdLinkContextMenu.ValueName);
                                AmdLinkDevices.CurrentValue = displayAdapterkey.GetValue(AmdLinkDevices.ValueName);
                                ReleaseNotes.CurrentValue = displayAdapterkey.GetValue(ReleaseNotes.ValueName);
                            }
                        }
                    }
                }
            }
        }
    }
}
