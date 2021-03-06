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
        private DirectoryInfo _cnDir;
        private IList<RunningHostServiceModel> _hostServices;

        //File paths and names
        private readonly string INSTALL_FOLDER_DEFAULT_PATH = Environment.SpecialFolder.ProgramFiles + @"\AMD\CNext\CNext";
        private const string INSTALL_FOLDER_REGISTRY_KEY = @"SOFTWARE\AMD\CN";
        private const string INSTALL_FOLDER_REGISTRY_VALUE_NAME = "InstallDir";
        private const string RADEON_SOFTWARE_CLI_FILE_NAME = "cncmd.exe";

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


        public void LoadOrRefresh()
        {
            LoadCnDirectory();
            LoadRunningHostServiceProcesses();
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
