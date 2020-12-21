using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Win32;
using RadeonSoftwareSlimmer.Services;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Models.PostInstall
{
    public class InstalledModel : INotifyPropertyChanged
    {
        private bool _uninstall;
        private readonly bool _windowsInstaller;
        private string _uninstallExe;
        private string _uninstallArguments;

        public InstalledModel(RegistryKey uninstallKey, string keyShortName)
        {
            Uninstall = false;

            //Again... no consistency or all the information filled out with this from AMD...
            DisplayName = GetRegistryValueString(uninstallKey, "DisplayName");
            Publisher = GetRegistryValueString(uninstallKey, "Publisher");
            DisplayVersion = GetRegistryValueString(uninstallKey, "DisplayVersion");
            UninstallCommand = GetRegistryValueString(uninstallKey, "UninstallString");
            ProductCode = keyShortName;

            string windowsInstaller = GetRegistryValueString(uninstallKey, "WindowsInstaller");
            if (!string.IsNullOrWhiteSpace(windowsInstaller))
            {
                _windowsInstaller = Convert.ToBoolean(int.Parse(windowsInstaller, CultureInfo.CurrentCulture));
            }

            DetermineUninstallCommand();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public bool Uninstall
        {
            get { return _uninstall; }
            set
            {
                _uninstall = value;
                OnPropertyChanged(nameof(RunUninstaller));
            }
        }
        public string DisplayName { get; }
        internal string Publisher { get; }
        public string ProductCode { get; }
        public string DisplayVersion { get; }
        public string UninstallCommand { get; private set; }


        public void RunUninstaller()
        {
            if (!string.IsNullOrWhiteSpace(UninstallCommand))
            {
                if (_windowsInstaller)
                {
                    ProcessHandler processHandler = new ProcessHandler(_uninstallExe);
                    processHandler.RunProcess($"{_uninstallArguments} /quiet /norestart REBOOT=ReallySuppress");
                }
                else
                {
                    StaticViewModel.AddDebugMessage($"Non-Windows uninstaller for {DisplayName} not supported");
                }
            }
            else
                StaticViewModel.AddDebugMessage($"No uninstaller command for {DisplayName}");
        }


        private static string GetRegistryValueString(RegistryKey registryKey, string valueName)
        {
            if (registryKey == null || string.IsNullOrWhiteSpace(valueName))
                return null;

            object value = registryKey.GetValue(valueName);
            if (value == null)
                return null;

            return value.ToString();
        }

        private void DetermineUninstallCommand()
        {
            if (_windowsInstaller)
            {
                _uninstallExe = Environment.GetFolderPath(Environment.SpecialFolder.System) +  "\\msiexec.exe";
                if (Guid.TryParse(ProductCode, out Guid productGuid))
                {
                    _uninstallArguments = $"/uninstall {productGuid:B}";
                    UninstallCommand = $"{_uninstallExe} {_uninstallArguments}";
                    StaticViewModel.AddDebugMessage($"Detected GUID {productGuid} from {ProductCode} for {DisplayName}");
                }
                else
                {
                    StaticViewModel.AddDebugMessage($"Unable to determine windows installer GUID from {ProductCode} for {DisplayName}");
                }
            }
            else if (!string.IsNullOrWhiteSpace(UninstallCommand))
            {
                StaticViewModel.AddDebugMessage($"Keeping default uninstall command {UninstallCommand} for {DisplayName}");
            }
            else
            {
                StaticViewModel.AddDebugMessage($"Unable to determine uninstall command for {DisplayName}");
            }
        }
    }
}
