using System;
using System.Collections.Generic;
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
        private IList<RunningHostServiceModel> _hostServices;
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


        public IList<RunningHostServiceModel> HostServices
        {
            get { return _hostServices; }
            set
            {
                _hostServices = value;
                OnPropertyChanged(nameof(HostServices));
            }
        }
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
            LoadRunningHostServiceProcesses();
            CheckIfHostServiceIsEnabled();
        }

        public void StopRadeonSoftware()
        {
            try
            {
                StaticViewModel.AddLogMessage("Stopping Radeon Software Host Services");
                StaticViewModel.IsLoading = true;

                RadeonSoftwareCli(CNCMD_EXIT);

                //The command to stop the services does not wait for them to fully end
                ProcessHandler hostProcess = new ProcessHandler(_cnDir.FullName + "AMDRSServ.exe");
                hostProcess.WaitForProcessToEnd(30);
                ProcessHandler radeonSoftwareProcess = new ProcessHandler(_cnDir.FullName + "RadeonSoftware.exe");
                radeonSoftwareProcess.WaitForProcessToEnd(30);

                StaticViewModel.AddLogMessage("Stopped Radeon Software Host Services");
            }
            catch (Exception ex)
            {
                StaticViewModel.AddLogMessage(ex, "Failed to stop Radeon Software Host Services");
            }
            finally
            {
                LoadOrRefresh();
                StaticViewModel.IsLoading = false;
            }
        }

        public void RestartRadeonSoftware()
        {
            try
            {
                StaticViewModel.AddLogMessage("Restarting Radeon Software Host Services");
                StaticViewModel.IsLoading = true;

                RadeonSoftwareCli(CNCMD_RESTART);

                //Wait for services to start back up
                ProcessHandler radeonSoftwareProcess = new ProcessHandler(_cnDir.FullName + "RadeonSoftware.exe");
                radeonSoftwareProcess.WaitForProcessToStart(30);

                if (Enabled)
                {
                    ProcessHandler hostProcess = new ProcessHandler(_cnDir.FullName + "AMDRSServ.exe");
                    hostProcess.WaitForProcessToStart(30);
                }

                StaticViewModel.AddLogMessage("Restarted Radeon Software Host Services");
            }
            catch (Exception ex)
            {
                StaticViewModel.AddLogMessage(ex, "Failed to restart Radeon Software Host Services");
            }
            finally
            {
                LoadOrRefresh();
                StaticViewModel.IsLoading = false;
            }
        }

        public void ApplyChanges()
        {
            if (Enabled)
                Enable();
            else
                Disable();
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
            if (_rsServFile.Exists)
            {
                if (_rsServDisabledFile.Exists)
                {
                    StaticViewModel.AddDebugMessage($"Deleting previous disable file ${_rsServDisabledFile.FullName}");
                    _rsServDisabledFile.Delete();
                }

                StaticViewModel.AddDebugMessage($"Renaming {_rsServFile.Name} to {_rsServDisabledFile.Name}");
                _rsServFile.MoveTo(_rsServDisabledFile.FullName);

                StaticViewModel.AddDebugMessage("Host Service Disabled");
                Enabled = false;
            }
        }


        private void LoadRunningHostServiceProcesses()
        {
            HostServices = new List<RunningHostServiceModel>
            {
                new RunningHostServiceModel("RadeonSoftware", "Radeon Software: Host Application"),
                new RunningHostServiceModel("AMDRSServ", "Radeon Settings: Host Service"),
                new RunningHostServiceModel("amdow", "Radeon Settings: Desktop Overlay"),
                new RunningHostServiceModel("AMDRSSrcExt", "Radeon Settings: Source Extension"),
            };
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
            ProcessHandler processHandler = new ProcessHandler(_cnDir.FullName + RADEON_SOFTWARE_CLI_FILE_NAME);
            processHandler.RunProcess(arugument);
        }
    }
}
