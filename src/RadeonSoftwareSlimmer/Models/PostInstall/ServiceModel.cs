using System;
using System.ComponentModel;
using System.ServiceProcess;
using RadeonSoftwareSlimmer.Services;

namespace RadeonSoftwareSlimmer.Models.PostInstall
{
    public class ServiceModel : INotifyPropertyChanged
    {
        private readonly bool _exists;
        private ServiceStartMode _startMode;

        public ServiceModel(string serviceName)
        {
            using (ServiceController serviceController = new ServiceController(serviceName))
            {
                try
                {
                    Name = serviceController.ServiceName;
                    DisplayName = serviceController.DisplayName;
                    _exists = true;
                    StartMode = serviceController.StartType;
                    Type = serviceController.ServiceType.ToString();
                    Status = serviceController.Status;
                }
                catch (InvalidOperationException)
                {
                    // Can't find a better way to handle servics that don't exist :(
                    _exists = false;
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public ServiceStartMode StartMode
        {
            get { return _startMode; }
            set
            {
                _startMode = value;
                OnPropertyChanged(nameof(StartMode));
            }
        }
        public string Name { get; }
        public string DisplayName { get; }
        public ServiceControllerStatus Status { get; }
        public string Type { get; }


        public bool Exists()
        {
            return _exists;
        }

        public void SetStartMode()
        {
            if (_exists)
            {
                using (ServiceController serviceController = new ServiceController(Name))
                {
                    serviceController.Refresh();

                    if (StartMode != serviceController.StartType)
                    {
                        //It's this or WMI...
                        ProcessHandler processHandler = new ProcessHandler(Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\sc.exe");
                        processHandler.RunProcess($"config \"{Name}\" start= {GetStartModeCommandString(StartMode)}");

                        serviceController.Refresh();
                        StartMode = serviceController.StartType;
                    }
                }
            }
        }


        private static string GetStartModeCommandString(ServiceStartMode serviceStartMode)
        {
            switch (serviceStartMode)
            {
                case ServiceStartMode.Boot:
                    return "boot";
                case ServiceStartMode.System:
                    return "system";
                case ServiceStartMode.Automatic:
                    return "auto";
                case ServiceStartMode.Manual:
                    return "demand";
                case ServiceStartMode.Disabled:
                    return "disabled";
                default:
                    return string.Empty;
            }
        }
    }
}
