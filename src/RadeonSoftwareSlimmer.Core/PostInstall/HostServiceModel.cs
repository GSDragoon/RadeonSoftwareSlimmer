using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Abstractions;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Core.PostInstall
{
    public class HostServiceModel : INotifyPropertyChanged
    {
        private readonly IAppLogger _logger;
        private readonly IFileSystem _fileSystem;
        private readonly IRegistry _registry;
        private readonly IProcessHandler _processHandler;
        private readonly IProcessRunner _processRunner;
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


        public HostServiceModel(IFileSystem fileSystem, IRegistry registry, IAppLogger logger, IProcessHandler processHandler, IProcessRunner processRunner)
        {
            _logger = logger;
            _fileSystem = fileSystem;
            _registry = registry;
            _processHandler = processHandler;
            _processRunner = processRunner;
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
                    _logger.Info("Stopping Radeon Software Host Services");
                    _logger.IsLoading = true;

                    RadeonSoftwareCli(CNCMD_EXIT);

                    //The command to stop the services does not wait for them to fully end
                    _processHandler.WaitForProcessToEnd("AMDRSServ", 30);
                    _processHandler.WaitForProcessToEnd("RadeonSoftware", 30);

                    _logger.Info("Stopped Radeon Software Host Services");
                }
                catch (Exception ex)
                {
                    _logger.Info(ex, "Failed to stop Radeon Software Host Services");
                }
                finally
                {
                    LoadOrRefresh();
                    _logger.IsLoading = false;
                }
            }
        }

        public void RestartRadeonSoftware()
        {
            if (_installed)
            {
                try
                {
                    _logger.Info("Restarting Radeon Software Host Services");
                    _logger.IsLoading = true;

                    RadeonSoftwareCli(CNCMD_RESTART);

                    //Wait for services to start back up
                    _processHandler.WaitForProcessToStart("RadeonSoftware", 30);


                    _logger.Info("Restarted Radeon Software Host Services");
                }
                catch (Exception ex)
                {
                    _logger.Info(ex, "Failed to restart Radeon Software Host Services");
                }
                finally
                {
                    LoadOrRefresh();
                    _logger.IsLoading = false;
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
                _logger.Debug($"Found Radeon Software installation directory at {registryDirectory}");
                _cnDir = _fileSystem.DirectoryInfo.New(registryDirectory);
                Installed = true;
            }
            else
            {
                // Then check the default location
                _logger.Debug("Unable to determine Radeon Software installation directory from registry. Checking default location.");
                string defaultDirectory = _fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "AMD", "CNext", "CNext");
                if (IsValidRadeonSoftwareDirectory(defaultDirectory))
                {
                    _logger.Debug($"Found Radeon Software installation directory at {defaultDirectory}");
                    _cnDir = _fileSystem.DirectoryInfo.New(defaultDirectory);
                    Installed = true;
                }
                else
                {
                    Installed = false;
                    _logger.Debug("Unable to determine Radeon Software installation directory from default location. Assuming it is not installed.");
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
                        _logger.Debug($"Unable to open registry key {INSTALL_FOLDER_REGISTRY_KEY}.");
                        return string.Empty;
                    }

                    object installDirObj = cnKey.GetValue(INSTALL_FOLDER_REGISTRY_VALUE_NAME);
                    if (installDirObj == null)
                    {
                        _logger.Debug($"Registry value {INSTALL_FOLDER_REGISTRY_VALUE_NAME} does not exist or is null.");
                        return string.Empty;
                    }

                    return installDirObj.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex);
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
                return _fileSystem.File.Exists(cliFile);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex);
                return false;
            }
        }

        private void RadeonSoftwareCli(string aruguments)
        {
            if (_installed)
                _processRunner.RunProcess(_fileSystem.Path.Combine(_cnDir.FullName, RADEON_SOFTWARE_CLI_FILE_NAME), aruguments);
        }
    }
}
