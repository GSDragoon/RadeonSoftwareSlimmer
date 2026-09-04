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
        private CoreServiceStartMode _startMode;
        private CoreServiceStartMode _originalStartMode;
        private CoreServiceControllerStatus _status;
        private readonly CoreServiceType _serviceType;

        private readonly string _scExe;

        private const string SERVICES_REG_KEY = @"SYSTEM\CurrentControlSet\Services\";
        private const string SERVICE_START_VALUE_NAME = "Start";
        private const string SERVICE_ORIGINAL_START_VALUE_NAME = "RadeonSoftwareSlimmerOriginalStart";

        public ServiceModel(string serviceName, IRegistry registry, IAppLogger logger, IProcessRunner processRunner, IServiceControllerFactory serviceControllerFactory)
        {
            _logger = logger;
            _registry = registry;
            _processRunner = processRunner;
            _serviceController = serviceControllerFactory.Create(serviceName);
            _exists = _serviceController.Exists;

            if (_exists)
            {
                Name = _serviceController.ServiceName;
                DisplayName = _serviceController.DisplayName;
                Enabled = _serviceController.StartType != CoreServiceStartMode.Disabled;
                StartMode = _serviceController.StartType;
                _serviceType = _serviceController.ServiceType;
                Status = _serviceController.Status;

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
        public CoreServiceStartMode StartMode
        {
            get { return _startMode; }
            private set
            {
                _startMode = value;
                OnPropertyChanged(nameof(StartMode));
            }
        }
        public CoreServiceControllerStatus Status
        {
            get { return _status; }
            private set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
        public string Type => _serviceType.ToString();
        public CoreServiceStartMode OriginalStartMode
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
            if (_startMode == CoreServiceStartMode.Disabled)
            {
                _logger.Info($"Cannot start {Name} because it is disabled");
                return;
            }

            if (_serviceType.HasFlag(CoreServiceType.KernelDriver))
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
                if (_serviceController.StartType != CoreServiceStartMode.Disabled && _serviceController.ServiceType.HasFlag(CoreServiceType.Win32OwnProcess))
                {
                    _serviceController.Start();
                    _serviceController.WaitForStatus(CoreServiceControllerStatus.Running, TimeSpan.FromSeconds(30));

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
            if (_serviceType.HasFlag(CoreServiceType.KernelDriver))
            {
                _logger.Info($"Cannot stop {Name} because it is a kernel driver");
                return;
            }

            try
            {
                _logger.Info("Stopping " + Name);
                _logger.IsLoading = true;

                _serviceController.Refresh();
                if (_serviceController.Status == CoreServiceControllerStatus.Running && _serviceController.ServiceType.HasFlag(CoreServiceType.Win32OwnProcess))
                {
                    try
                    {
                        _serviceController.Stop();
                        _serviceController.WaitForStatus(CoreServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
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
            if (StartMode == CoreServiceStartMode.Disabled && OriginalStartMode != CoreServiceStartMode.Disabled)
            {
                //It's this or WMI...
                _processRunner.RunProcess(_scExe, $"config \"{Name}\" start= {GetStartModeCommandString(OriginalStartMode)}");

                if (_serviceType == CoreServiceType.Win32OwnProcess)
                    TryStart();

                _serviceController.Refresh();
                StartMode = _serviceController.StartType;
                Enabled = _serviceController.StartType != CoreServiceStartMode.Disabled;
            }
        }

        public void Disable()
        {
            _serviceController.Refresh();
            if (StartMode != CoreServiceStartMode.Disabled)
            {
                if (_serviceType.HasFlag(CoreServiceType.Win32OwnProcess))
                    TryStop();

                //It's this or WMI...
                _processRunner.RunProcess(_scExe, $"config \"{Name}\" start= {GetStartModeCommandString(CoreServiceStartMode.Disabled)}");

                _serviceController.Refresh();
                StartMode = _serviceController.StartType;
                Enabled = _serviceController.StartType != CoreServiceStartMode.Disabled;
            }
        }

        public void SetStartMode(string startMode)
        {
            CoreServiceStartMode serviceStartMode = (CoreServiceStartMode)Enum.Parse(typeof(CoreServiceStartMode), startMode);
            SetStartMode(serviceStartMode);
        }


        private void SetStartMode(CoreServiceStartMode startMode)
        {
            //It's this or WMI...
            _processRunner.RunProcess(_scExe, $"config \"{Name}\" start= {GetStartModeCommandString(startMode)}");

            _serviceController.Refresh();
            StartMode = _serviceController.StartType;
            Enabled = _serviceController.StartType != CoreServiceStartMode.Disabled;

            _logger.Info($"Changed start mode for {Name} to {StartMode}");
        }

        private static string GetStartModeCommandString(CoreServiceStartMode serviceStartMode)
        {
            switch (serviceStartMode)
            {
                case CoreServiceStartMode.Boot:
                    return "boot";
                case CoreServiceStartMode.System:
                    return "system";
                case CoreServiceStartMode.Automatic:
                    return "auto";
                case CoreServiceStartMode.Manual:
                    return "demand";
                case CoreServiceStartMode.Disabled:
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
                    OriginalStartMode = (CoreServiceStartMode)original;
                else
                    _logger.Debug("Unable to determin original start mode");

            }
        }

        private void SaveOriginalStartMode()
        {
            using (IRegistryKey serviceKey = _registry.LocalMachine.OpenSubKey(SERVICES_REG_KEY + Name, true))
            {
                object currentStartMode = serviceKey.GetValue(SERVICE_START_VALUE_NAME);
                serviceKey.SetValue(SERVICE_ORIGINAL_START_VALUE_NAME, currentStartMode, CoreRegistryValueKind.DWord);
            }
        }
    }
}
