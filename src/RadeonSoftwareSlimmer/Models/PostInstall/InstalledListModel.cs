using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Win32;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Models.PostInstall
{
    public class InstalledListModel : INotifyPropertyChanged
    {
        private IEnumerable<InstalledModel> _installedItems;

        private const string UNINSTALL_REGISTRY_PATH = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        private const string UNINSTALL_REGISTRY_PATH_WOW6432Node = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
        private readonly string[] AMD_CHIPSET_NAMES =
        {
            "Chipset",
            "GPIO",
            "PCI",
            "PSP",
            "Ryzen",
            "SMBus",
        };


        public InstalledListModel() { }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public IEnumerable<InstalledModel> InstalledItems
        {
            get { return _installedItems; }
            set
            {
                _installedItems = value;
                OnPropertyChanged(nameof(InstalledItems));
            }
        }


        public void LoadOrRefresh()
        {
            InstalledItems = new List<InstalledModel>(GetAllRadeonScheduledTasks());
        }

        public void ApplyChanges()
        {
            foreach (InstalledModel install in _installedItems)
            {
                if (install.Uninstall)
                    install.RunUninstaller();
            }
        }


        private IEnumerable<InstalledModel> GetAllRadeonScheduledTasks()
        {
            using (RegistryKey uninstallRootKey = Registry.LocalMachine.OpenSubKey(UNINSTALL_REGISTRY_PATH))
            {
                foreach (string uninstallName in uninstallRootKey.GetSubKeyNames())
                {
                    using (RegistryKey uninstallKey = uninstallRootKey.OpenSubKey(uninstallName))
                    {
                        if (IsRadeonUninstall(uninstallKey))
                        {
                            yield return new InstalledModel(uninstallKey, uninstallName);
                        }
                    }
                }
            }

            if (Environment.Is64BitOperatingSystem)
            {
                using (RegistryKey uninstallRootKey = Registry.LocalMachine.OpenSubKey(UNINSTALL_REGISTRY_PATH_WOW6432Node))
                {
                    foreach (string uninstallName in uninstallRootKey.GetSubKeyNames())
                    {
                        using (RegistryKey uninstallKey = uninstallRootKey.OpenSubKey(uninstallName))
                        {
                            if (IsRadeonUninstall(uninstallKey))
                            {
                                yield return new InstalledModel(uninstallKey, uninstallName);
                            }
                        }
                    }
                }
            }
        }

        private bool IsRadeonUninstall(RegistryKey uninstallKey)
        {
            object publisher = uninstallKey.GetValue("Publisher");
            object displayName = uninstallKey.GetValue("DisplayName");

            if (publisher != null && displayName != null
                && publisher.ToString().Equals("Advanced Micro Devices, Inc.", StringComparison.OrdinalIgnoreCase)
                && !AMD_CHIPSET_NAMES.Any(name => displayName.ToString().Contains(name)))
            {
                StaticViewModel.AddDebugMessage($"Found uninstall item {displayName} under {uninstallKey.Name}");
                return true;
            }

            return false;
        }
    }
}
