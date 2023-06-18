using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Abstractions;
using RadeonSoftwareSlimmer.Intefaces;
using RadeonSoftwareSlimmer.Services;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Models.PostInstall
{
    public class HostServiceModel : INotifyPropertyChanged
    {
        private readonly IFileSystem _fileSystem;
        private readonly IRegistry _registry;
        private IDirectoryInfo _cnDir;
        private bool _installed;
        private IList<RunningHostServiceModel> _hostServices;

        //File paths and names
        private const string INSTALL_FOLDER_REGISTRY_KEY = @"SOFTWARE\AMD\CN";
        private const string INSTALL_FOLDER_REGISTRY_VALUE_NAME = "InstallDir";
        private const string RADEON_SOFTWARE_CLI_FILE_NAME = "cncmd.exe";

        //cncmd.exe commands
        public const string CNCMD_EXIT = "exit";
        public const string CNCMD_RESTART = "restart";


        public HostServiceModel(IFileSystem fileSystem, IRegistry registry)
        {
            _fileSystem = fileSystem;
            _registry = registry;
            Installed = false;
        }


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

        public bool Installed
        {
            get { return _installed; }
            private set
            {
                _installed = value;
                OnPropertyChanged(nameof(Installed));
            }
        }


        public void LoadOrRefresh()
        {
            LoadCnDirectory();
            LoadRunningHostServiceProcesses();
        }

        public void StopRadeonSoftware()
        {
            if (_installed)
            {
                try
                {
                    StaticViewModel.AddLogMessage("Stopping Radeon Software Host Services");
                    StaticViewModel.IsLoading = true;

                    RadeonSoftwareCli(CNCMD_EXIT);

                    //The command to stop the services does not wait for them to fully end
                    ProcessHandler hostProcess = new ProcessHandler(_fileSystem.Path.Combine(_cnDir.FullName, "AMDRSServ.exe"));
                    hostProcess.WaitForProcessToEnd(30);
                    ProcessHandler radeonSoftwareProcess = new ProcessHandler(_fileSystem.Path.Combine(_cnDir.FullName, "RadeonSoftware.exe"));
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
        }

        public void RestartRadeonSoftware()
        {
            if (_installed)
            {
                try
                {
                    StaticViewModel.AddLogMessage("Restarting Radeon Software Host Services");
                    StaticViewModel.IsLoading = true;

                    RadeonSoftwareCli(CNCMD_RESTART);

                    //Wait for services to start back up
                    ProcessHandler radeonSoftwareProcess = new ProcessHandler(_fileSystem.Path.Combine(_cnDir.FullName, "RadeonSoftware.exe"));
                    radeonSoftwareProcess.WaitForProcessToStart(30);


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

        private void LoadCnDirectory()
        {
            Installed = false;

            // Check registry first
            string registryDirectory = GetInstallDirectoryFromRegistry();
            if (IsValidRadeonSoftwareDirectory(registryDirectory))
            {
                StaticViewModel.AddDebugMessage($"Found Radeon Software installation directory at {registryDirectory}");
                _cnDir = _fileSystem.DirectoryInfo.New(registryDirectory);
                Installed = true;
            }
            else
            {
                // Then check the default location
                StaticViewModel.AddDebugMessage("Unable to determine Radeon Software installation directory from registry. Checking default location.");
                string defaultDirectory = _fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "AMD", "CNext", "CNext");
                if (IsValidRadeonSoftwareDirectory(defaultDirectory))
                {
                    StaticViewModel.AddDebugMessage($"Found Radeon Software installation directory at {defaultDirectory}");
                    _cnDir = _fileSystem.DirectoryInfo.New(defaultDirectory);
                    Installed = true;
                }
                else
                {
                    Installed = false;
                    StaticViewModel.AddDebugMessage("Unable to determine Radeon Software installation directory from default location. Assuming it is not installed.");
                }
            }
        }

        private string GetInstallDirectoryFromRegistry()
        {
            try
            {
                using (IRegistryKey cnKey = _registry.LocalMachine.OpenSubKey(INSTALL_FOLDER_REGISTRY_KEY, false))
                {
                    if (cnKey == null)
                    {
                        StaticViewModel.AddDebugMessage($"Unable to open registry key {INSTALL_FOLDER_REGISTRY_KEY}.");
                        return string.Empty;
                    }

                    object installDirObj = cnKey.GetValue(INSTALL_FOLDER_REGISTRY_VALUE_NAME);
                    if (installDirObj == null)
                    {
                        StaticViewModel.AddDebugMessage($"Registry value {INSTALL_FOLDER_REGISTRY_VALUE_NAME} does not exist or is null.");
                        return string.Empty;
                    }

                    return installDirObj.ToString();
                }
            }
            catch (Exception ex)
            {
                StaticViewModel.AddDebugMessage(ex);
                return string.Empty;
            }
        }

        private bool IsValidRadeonSoftwareDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
                return false;

            try
            {
                if (!_fileSystem.Directory.Exists(directory))
                    return false;

                string cliFile = _fileSystem.Path.Combine(directory, RADEON_SOFTWARE_CLI_FILE_NAME);
                if (!_fileSystem.File.Exists(cliFile))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                StaticViewModel.AddDebugMessage(ex);
                return false;
            }
        }

        private void RadeonSoftwareCli(string arugument)
        {
            if (_installed)
            {
                string cli = _fileSystem.Path.Combine(_cnDir.FullName, RADEON_SOFTWARE_CLI_FILE_NAME);
                ProcessHandler processHandler = new ProcessHandler(cli);
                processHandler.RunProcess(arugument);
            }
        }
    }
}
