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
        private readonly IServiceController _serviceController;
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

        public ServiceModel(string serviceName, IRegistry registry, IAppLogger logger, IProcessRunner processRunner, IServiceController serviceController)
        {
            _logger = logger;
            _registry = registry;
            _processRunner = processRunner;
            _serviceController = serviceController;

            _serviceController.Load(serviceName);
            _exists = _serviceController.Exists;

            if (_exists)
            {
                Name = serviceController.ServiceName;
                DisplayName = serviceController.DisplayName;
                Enabled = serviceController.StartType != ServiceStartMode.Disabled;
                StartMode = serviceController.StartType;
                _serviceType = serviceController.ServiceType;
                Status = serviceController.Status;

                LoadOriginalStartMode();

                _scExe = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "sc.exe");
            }
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

                _serviceController.Refresh();
                if (_serviceController.StartType != ServiceStartMode.Disabled && _serviceController.ServiceType.HasFlag(ServiceType.Win32OwnProcess))
                {
                    _serviceController.Start();
                    _serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));

                    Status = _serviceController.Status;
                    _logger.Info("Restarted " + Name);
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

                _serviceController.Refresh();
                if (_serviceController.Status == ServiceControllerStatus.Running && _serviceController.ServiceType.HasFlag(ServiceType.Win32OwnProcess))
                {
                    try
                    {
                        _serviceController.Stop();
                        _serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger.Debug(ex);
                    }

                    Status = _serviceController.Status;
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

        // Consider moving all this below to IServiceController instead of here

        public void Delete()
        {
            TryStop();

            _processRunner.RunProcess(_scExe, $"delete \"{Name}\"");
            _serviceController.Refresh();
        }

        public void Enable()
        {
            _serviceController.Refresh();
            if (StartMode == ServiceStartMode.Disabled && OriginalStartMode != ServiceStartMode.Disabled)
            {
                //It's this or WMI...
                _processRunner.RunProcess(_scExe, $"config \"{Name}\" start= {GetStartModeCommandString(OriginalStartMode)}");

                if (_serviceType == ServiceType.Win32OwnProcess)
                    TryStart();

                _serviceController.Refresh();
                StartMode = _serviceController.StartType;
                Enabled = _serviceController.StartType != ServiceStartMode.Disabled;
            }
        }

        public void Disable()
        {
            _serviceController.Refresh();
            if (StartMode != ServiceStartMode.Disabled)
            {
                if (_serviceType.HasFlag(ServiceType.Win32OwnProcess))
                    TryStop();

                //It's this or WMI...
                _processRunner.RunProcess(_scExe, $"config \"{Name}\" start= {GetStartModeCommandString(ServiceStartMode.Disabled)}");

                _serviceController.Refresh();
                StartMode = _serviceController.StartType;
                Enabled = _serviceController.StartType != ServiceStartMode.Disabled;
            }
        }

        public void SetStartMode(string startMode)
        {
            ServiceStartMode serviceStartMode = (ServiceStartMode)Enum.Parse(typeof(ServiceStartMode), startMode);
            SetStartMode(serviceStartMode);
        }


        private void SetStartMode(ServiceStartMode startMode)
        {
            //It's this or WMI...
            _processRunner.RunProcess(_scExe, $"config \"{Name}\" start= {GetStartModeCommandString(startMode)}");

            _serviceController.Refresh();
            StartMode = _serviceController.StartType;
            Enabled = _serviceController.StartType != ServiceStartMode.Disabled;

            _logger.Info($"Changed start mode for {Name} to {StartMode}");
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
