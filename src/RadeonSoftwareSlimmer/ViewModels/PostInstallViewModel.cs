using System;
using System.ComponentModel;
using System.Threading.Tasks;
using RadeonSoftwareSlimmer.Models.PostInstall;

namespace RadeonSoftwareSlimmer.ViewModels
{
    public class PostInstallViewModel : INotifyPropertyChanged
    {
        private bool _loadedPanelEnabled;

        public PostInstallViewModel()
        {
            LoadedPanelEnabled = false;

            HostService = new HostServiceModel();
            RadeonScheduledTaskList = new ScheduledTaskListModel();
            ServiceList = new ServiceListModel();
            InstalledList = new InstalledListModel();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public HostServiceModel HostService { get; }
        public ScheduledTaskListModel RadeonScheduledTaskList { get; }
        public ServiceListModel ServiceList { get; }
        public InstalledListModel InstalledList { get; }
        public bool LoadedPanelEnabled
        {
            get { return _loadedPanelEnabled; }
            private set
            {
                _loadedPanelEnabled = value;
                OnPropertyChanged(nameof(LoadedPanelEnabled));
            }
        }


        public void LoadOrRefresh()
        {
            try
            {
                StaticViewModel.AddLogMessage("Loading post install");
                StaticViewModel.IsLoading = true;
                LoadedPanelEnabled = false;

                HostService.LoadOrRefresh();
                RadeonScheduledTaskList.LoadOrRefresh();
                ServiceList.LoadOrRefresh();
                InstalledList.LoadOrRefresh();

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

        public async Task ApplyyChangesAsync()
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

                if (HostService.Enabled)
                    HostService.Enable();
                else
                    HostService.Disable();

                foreach (ScheduledTaskModel scheduledTask in RadeonScheduledTaskList.RadeonScheduledTasks)
                {
                    if (scheduledTask.Enabled)
                        scheduledTask.Enable();
                    else
                        scheduledTask.Disable();
                }

                foreach (ServiceModel service in ServiceList.Services)
                {
                    service.SetStartMode();
                }

                foreach (InstalledModel install in InstalledList.InstalledItems)
                {
                    install.UninstallIfSelected();
                }


                if (HostService.Enabled)
                    HostService.RestartRadeonSoftware();

                LoadOrRefresh();
                StaticViewModel.AddLogMessage("Changes applied to post install");
            }
            catch (Exception ex)
            {
                StaticViewModel.AddLogMessage(ex, "Failed to apply post install changes");
            }
            finally
            {
                StaticViewModel.IsLoading = false;
            }
        }
    }
}
