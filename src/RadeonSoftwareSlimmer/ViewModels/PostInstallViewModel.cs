using System;
using System.ComponentModel;
using System.IO.Abstractions;
using System.Threading.Tasks;
using RadeonSoftwareSlimmer.Intefaces;
using RadeonSoftwareSlimmer.Models.PostInstall;

namespace RadeonSoftwareSlimmer.ViewModels
{
    public class PostInstallViewModel : INotifyPropertyChanged
    {
        private bool _loadedPanelEnabled;

        public PostInstallViewModel(IFileSystem fileSystem, IRegistry registry)
        {
            LoadedPanelEnabled = false;

            HostService = new HostServiceModel(fileSystem, registry);
            WindowsAppStartup = new WindowsAppStartupModel(registry);
            RadeonScheduledTaskList = new ScheduledTaskListModel();
            ServiceList = new ServiceListModel(registry);
            InstalledList = new InstalledListModel(registry);
            TempFileList = new TempFileListModel(fileSystem);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public HostServiceModel HostService { get; }
        public WindowsAppStartupModel WindowsAppStartup { get; set; }
        public ScheduledTaskListModel RadeonScheduledTaskList { get; }
        public ServiceListModel ServiceList { get; }
        public InstalledListModel InstalledList { get; }
        public TempFileListModel TempFileList { get; }
        public bool LoadedPanelEnabled
        {
            get { return _loadedPanelEnabled; }
            private set
            {
                _loadedPanelEnabled = value;
                OnPropertyChanged(nameof(LoadedPanelEnabled));
            }
        }

        public async Task LoadOrRefreshAsync(bool logMessage)
        {
            await Task.Run(() => LoadOrRefresh(logMessage));
        }
        private void LoadOrRefresh(bool logMessage)
        {
            try
            {
                if (logMessage)
                    StaticViewModel.AddLogMessage("Loading post install");

                StaticViewModel.IsLoading = true;
                LoadedPanelEnabled = false;

                HostService.LoadOrRefresh();
                WindowsAppStartup.LoadOrRefresh();
                RadeonScheduledTaskList.LoadOrRefresh();
                ServiceList.LoadOrRefresh();
                InstalledList.LoadOrRefresh();
                TempFileList.LoadOrRefresh();

                if (logMessage)
                    StaticViewModel.AddLogMessage("Loading post install complete ");

                LoadedPanelEnabled = true;
            }
            catch (Exception ex)
            {
                StaticViewModel.AddLogMessage(ex);
            }
            finally
            {
                StaticViewModel.IsLoading = false;
            }
        }

        public async Task ApplyChangesAsync()
        {
            await Task.Run(() => ApplyChanges());
        }
        private void ApplyChanges()
        {
            try
            {
                StaticViewModel.IsLoading = true;
                LoadedPanelEnabled = false;
                StaticViewModel.AddLogMessage("Applying changes post install");

                HostService.StopRadeonSoftware();

                WindowsAppStartup.ApplyChanges();
                RadeonScheduledTaskList.ApplyChanges();
                ServiceList.ApplyChanges();
                InstalledList.ApplyChanges();
                TempFileList.ApplyChanges();

                StaticViewModel.AddLogMessage("Changes applied to post install");
            }
            catch (Exception ex)
            {
                StaticViewModel.AddLogMessage(ex, "Failed to apply post install changes");
            }
            finally
            {
                LoadOrRefresh(false);
                StaticViewModel.IsLoading = false;
            }
        }


        public async Task HostServices_StopAsync()
        {
            await Task.Run(() => HostServices_Stop());
        }
        private void HostServices_Stop()
        {
            HostService.StopRadeonSoftware();
            HostService.LoadOrRefresh();
        }

        public async Task HostServices_RestartAsync()
        {
            await Task.Run(() => HostServices_Restart());
        }
        private void HostServices_Restart()
        {
            HostService.RestartRadeonSoftware();
            HostService.LoadOrRefresh();
        }


        public void ScheduledTask_SetAll(bool enabled)
        {
            foreach (ScheduledTaskModel scheduledTask in RadeonScheduledTaskList.RadeonScheduledTasks)
            {
                scheduledTask.Enabled = enabled;
            }
        }


        public static async Task Service_StopAsync(object selectedService)
        {
            ServiceModel service = (ServiceModel)selectedService;
            if (service != null)
                await Task.Run(() => service.TryStop());
        }
        public static async Task Service_StartAsync(object selectedService)
        {
            ServiceModel service = (ServiceModel)selectedService;
            if (service != null)
                await Task.Run(() => service.TryStart());
        }
        public static async Task Service_RestartAsync(object selectedService)
        {
            await Service_StopAsync(selectedService);
            await Service_StartAsync(selectedService);
        }
        public static async Task Service_DeleteAsync(object selectedService)
        {
            ServiceModel service = (ServiceModel)selectedService;
            if (service != null)
                await Task.Run(() => service.Delete());
        }
        public static async Task Service_SetStartModeAsync(object selectedService, string selectedStartMode)
        {
            ServiceModel service = (ServiceModel)selectedService;
            if (service != null)
                await Task.Run(() => service.SetStartMode(selectedStartMode));
        }


        public void TempFilesSetAll(bool clear)
        {
            foreach (TempFileModel tempFile in TempFileList.TempFiles)
            {
                tempFile.Clear = clear;
            }
        }
    }
}
