using System;
using System.ComponentModel;
using System.IO;
using Microsoft.Win32;
using RadeonSoftwareSlimmer.Services;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Models.PostInstall
{
    public class HostServiceModel : INotifyPropertyChanged
    {
        private bool _enabled;
        private DirectoryInfo _cnDir;
        private FileInfo _rsServFile;
        private FileInfo _rsServDisabledFile;

        //File paths and names
        private readonly string INSTALL_FOLDER_DEFAULT_PATH = Environment.SpecialFolder.ProgramFiles + @"\AMD\CNext\CNext";
        private const string INSTALL_FOLDER_REGISTRY_KEY = @"SOFTWARE\AMD\CN";
        private const string INSTALL_FOLDER_REGISTRY_VALUE_NAME = "InstallDir";
        private const string RADEON_SOFTWARE_CLI_FILE_NAME = "cncmd.exe";
        private const string HOST_SERVICE_DISABLED_FILE_NAME = "RSServCmd_RadeonSoftwareSlimmerDisabled.exe";
        private const string HOST_SERVICE_FILE_NAME = "RSServCmd.exe";

        //cncmd.exe commands
        public const string CNCMD_EXIT = "exit";
        public const string CNCMD_RESTART = "restart";


        public HostServiceModel() { }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }


        public void LoadOrRefresh()
        {
            LoadCnDirectory();
            CheckIfHostServiceIsEnabled();
        }

        public void StopRadeonSoftware()
        {
            StaticViewModel.AddDebugMessage("Stopping Radeon Software Host Service");
            RadeonSoftwareCli(CNCMD_EXIT);
        }

        public void RestartRadeonSoftware()
        {
            StaticViewModel.AddDebugMessage("Restarting Radeon Software Host Service");
            RadeonSoftwareCli(CNCMD_RESTART);
        }

        public void Enable()
        {
            if (_rsServDisabledFile.Exists)
            {
                StaticViewModel.AddDebugMessage($"{_rsServDisabledFile.FullName} exists, attmpting to restore default file name");

                if (_rsServFile.Exists)
                {
                    StaticViewModel.AddDebugMessage($"{_rsServFile.FullName} already exists. Leaving files alone.");
                }
                else
                {
                    StaticViewModel.AddDebugMessage($"Renaming {_rsServDisabledFile.Name} to {_rsServFile.Name}");
                    _rsServDisabledFile.MoveTo(_rsServFile.FullName);
                }

                StaticViewModel.AddDebugMessage("Host Service Enabled");
                Enabled = true;
            }
        }

        public void Disable()
        {
            if (_rsServDisabledFile.Exists)
            {
                StaticViewModel.AddDebugMessage($"Deleting previous disable file ${_rsServDisabledFile.FullName}");
                _rsServDisabledFile.Delete();
            }

            if (_rsServFile.Exists)
            {
                StaticViewModel.AddDebugMessage($"Renaming {_rsServFile.Name} to {_rsServDisabledFile.Name}");
                _rsServFile.MoveTo(_rsServDisabledFile.FullName);
            }

            StaticViewModel.AddDebugMessage("Host Service Disabled");
            Enabled = false;
        }


        private void CheckIfHostServiceIsEnabled()
        {
            _rsServFile = new FileInfo(_cnDir.FullName + HOST_SERVICE_FILE_NAME);
            _rsServDisabledFile = new FileInfo(_cnDir + HOST_SERVICE_DISABLED_FILE_NAME);
            Enabled = _rsServFile.Exists;

            StaticViewModel.AddDebugMessage($"{_rsServFile.FullName} Exists: {_rsServFile.Exists}");
            StaticViewModel.AddDebugMessage($"{_rsServDisabledFile.FullName} Exists: {_rsServDisabledFile.Exists}");
        }

        private void LoadCnDirectory()
        {
            using (RegistryKey cnKey = Registry.LocalMachine.OpenSubKey(INSTALL_FOLDER_REGISTRY_KEY))
            {
                if (cnKey != null)
                {
                    string installDir = cnKey.GetValue(INSTALL_FOLDER_REGISTRY_VALUE_NAME).ToString();

                    if (string.IsNullOrWhiteSpace(installDir))
                    {
                        _cnDir = new DirectoryInfo(INSTALL_FOLDER_DEFAULT_PATH);
                        StaticViewModel.AddDebugMessage($"Unable to read {INSTALL_FOLDER_REGISTRY_VALUE_NAME} from {INSTALL_FOLDER_REGISTRY_KEY}. Defaulting to {INSTALL_FOLDER_DEFAULT_PATH}.");
                    }
                    else
                    {
                        _cnDir = new DirectoryInfo(installDir);
                        StaticViewModel.AddDebugMessage($"Found {installDir} from {INSTALL_FOLDER_REGISTRY_KEY}.");
                    }
                }
                else
                {
                    _cnDir = new DirectoryInfo(INSTALL_FOLDER_DEFAULT_PATH);
                    StaticViewModel.AddDebugMessage($"Unable to read from {INSTALL_FOLDER_REGISTRY_KEY}. Defaulting to {INSTALL_FOLDER_DEFAULT_PATH}.");
                }
            }
        }

        private void RadeonSoftwareCli(string arugument)
        {
            ProcessExecutor.RunProcess(_cnDir.FullName + RADEON_SOFTWARE_CLI_FILE_NAME, arugument);
        }
    }
}
