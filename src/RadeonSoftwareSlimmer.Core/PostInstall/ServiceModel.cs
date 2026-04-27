using System;
using System.ComponentModel;
using RadeonSoftwareSlimmer.Core.Enums;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Core.PostInstall
{
    public class ServiceModel : INotifyPropertyChanged
    {
        private readonly IRegistry _registry;
        private readonly IAppLogger _logger;
        private readonly IProcessRunner _processRunner;
        private readonly bool _exists;
        private bool _enabled;
        private ServiceStartMode _startMode;
        private ServiceStartMode _originalStartMode;
        private ServiceControllerStatus _status;
        private readonly ServiceType _serviceType;

        private readonly string _scExe;

        private const string SERVICES_REG_KEY = @"SYSTEM\CurrentControlSet\Services\";
        private const string SERVICE_START_VALUE_NAME = "Start";
        private const string SERVICE_ORIGINAL_START_VALUE_NAME = "RadeonSoftwareSlimmerOriginalStart";

        public ServiceModel(string serviceName, IRegistry registry, IAppLogger logger, IProcessRunner processRunner)
        {
            _logger = logger;
            _registry = registry;
            _processRunner = processRunner;

            using (ServiceController serviceController = new ServiceController(serviceName))
            {
                try
                {
                    Name = serviceController.ServiceName;
                    DisplayName = serviceController.DisplayName;
                    _exists = true;
                    Enabled = serviceController.StartType != ServiceStartMode.Disabled;
                    StartMode = serviceController.StartType;
                    _serviceType = serviceController.ServiceType;
                    Status = serviceController.Status;

                    LoadOriginalStartMode();
                }
                catch (InvalidOperationException)
                {
                    // Can't find a better way to handle servics that don't exist :(
                    _exists = false;
                }
            }

            _scExe = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "sc.exe");
        }


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
        public string Name { get; }
        public string DisplayName { get; }
        public ServiceStartMode StartMode
        {
            get { return _startMode; }
            private set
            {
                _startMode = value;
                OnPropertyChanged(nameof(StartMode));
            }
        }
        public ServiceControllerStatus Status
        {
            get { return _status; }
            private set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
        public string Type => _serviceType.ToString();
        public ServiceStartMode OriginalStartMode
        {
            get { return _originalStartMode; }
            private set
            {
                _originalStartMode = value;
                OnPropertyChanged(nameof(OriginalStartMode));
            }
        }


        public bool Exists()
        {
            return _exists;
        }

        public void TryStart()
        {
            if (_startMode == ServiceStartMode.Disabled)
            {
                _logger.Info($"Cannot start {Name} because it is disabled");
                return;
            }

            if (_serviceType.HasFlag(ServiceType.KernelDriver))
            {
                _logger.Info($"Cannot start {Name} because it is a kernel driver");
                return;
            }

            try
            {
                _logger.Info("Restarting " + Name);
                _logger.IsLoading = true;

                TryStop();

                using (ServiceController serviceController = LoadFreshService())
                {
                    if (serviceController.StartType != ServiceStartMode.Disabled && serviceController.ServiceType.HasFlag(ServiceType.Win32OwnProcess))
                    {
                        serviceController.Start();
                        serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));

                        Status = serviceController.Status;
                        _logger.Info("Restarted " + Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Info(ex, "Failed to restart " + Name);
            }
            finally
            {
                _logger.IsLoading = false;
            }
        }

        public void TryStop()
        {
            if (_serviceType.HasFlag(ServiceType.KernelDriver))
            {
                _logger.Info($"Cannot stop {Name} because it is a kernel driver");
                return;
            }

            try
            {
                _logger.Info("Stopping " + Name);
                _logger.IsLoading = true;

                using (ServiceController serviceController = LoadFreshService())
                {
                    if (serviceController.Status == ServiceControllerStatus.Running && serviceController.ServiceType.HasFlag(ServiceType.Win32OwnProcess))
                    {
                        try
                        {
                            serviceController.Stop();
                            serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                        }
                        catch (InvalidOperationException ex)
                        {
                            _logger.Debug(ex);
                        }

                        Status = serviceController.Status;
                    }
                }

                _logger.Info("Stopped " + Name);
            }
            catch (Exception ex)
            {
                _logger.Info(ex, "Failed to stop " + Name);
            }
            finally
            {
                _logger.IsLoading = false;
            }
        }

        public void Delete()
        {
            TryStop();

            using (ServiceController serviceController = LoadFreshService())
            {
                _processRunner.RunProcess(_scExe, $"delete \"{Name}\"");
                //Should delete the driver from the driver store and uninstall using pnputil
                serviceController.Refresh();
            }
        }

        public void Enable()
        {
            using (ServiceController serviceController = LoadFreshService())
            {
                if (StartMode == ServiceStartMode.Disabled && OriginalStartMode != ServiceStartMode.Disabled)
                {
                    //It's this or WMI...
                    _processRunner.RunProcess(_scExe, $"config \"{Name}\" start= {GetStartModeCommandString(OriginalStartMode)}");

                    if (_serviceType == ServiceType.Win32OwnProcess)
                        TryStart();

                    serviceController.Refresh();
                    StartMode = serviceController.StartType;
                    Enabled = serviceController.StartType != ServiceStartMode.Disabled;
                }
            }
        }

        public void Disable()
        {
            using (ServiceController serviceController = LoadFreshService())
            {
                if (StartMode != ServiceStartMode.Disabled)
                {
                    if (_serviceType.HasFlag(ServiceType.Win32OwnProcess))
                        TryStop();

                    //It's this or WMI...
                    _processRunner.RunProcess(_scExe, $"config \"{Name}\" start= {GetStartModeCommandString(ServiceStartMode.Disabled)}");

                    serviceController.Refresh();
                    StartMode = serviceController.StartType;
                    Enabled = serviceController.StartType != ServiceStartMode.Disabled;
                }
            }
        }

        public void SetStartMode(string startMode)
        {
            ServiceStartMode serviceStartMode = (ServiceStartMode)Enum.Parse(typeof(ServiceStartMode), startMode);
            SetStartMode(serviceStartMode);
        }


        private void SetStartMode(ServiceStartMode startMode)
        {
            using (ServiceController serviceController = LoadFreshService())
            {
                //It's this or WMI...
                _processRunner.RunProcess(_scExe, $"config \"{Name}\" start= {GetStartModeCommandString(startMode)}");

                serviceController.Refresh();
                StartMode = serviceController.StartType;
                Enabled = serviceController.StartType != ServiceStartMode.Disabled;

                _logger.Info($"Changed start mode for {Name} to {StartMode}");
            }
        }

        private ServiceController LoadFreshService()
        {
            if (_exists)
            {
                ServiceController serviceController = new ServiceController(Name);
                serviceController.Refresh();

                return serviceController;
            }

            return new ServiceController();
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

        private void LoadOriginalStartMode()
        {
            using (IRegistryKey serviceKey = _registry.LocalMachine.OpenSubKey(SERVICES_REG_KEY + Name, false))
            {
                object original = serviceKey.GetValue(SERVICE_ORIGINAL_START_VALUE_NAME, null);

                if (original == null)
                {
                    SaveOriginalStartMode();
                    original = serviceKey.GetValue(SERVICE_ORIGINAL_START_VALUE_NAME, null);
                }

                if (original != null)
                    OriginalStartMode = (ServiceStartMode)original;
                else
                    _logger.Debug("Unable to determin original start mode");

            }
        }

        private void SaveOriginalStartMode()
        {
            using (IRegistryKey serviceKey = _registry.LocalMachine.OpenSubKey(SERVICES_REG_KEY + Name, true))
            {
                object currentStartMode = serviceKey.GetValue(SERVICE_START_VALUE_NAME);
                serviceKey.SetValue(SERVICE_ORIGINAL_START_VALUE_NAME, currentStartMode, RegistryValueKind.DWord);
            }
        }
    }
}
