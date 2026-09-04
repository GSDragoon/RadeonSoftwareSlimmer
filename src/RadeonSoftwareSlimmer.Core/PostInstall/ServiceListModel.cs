using System.Collections.Generic;
using System.ComponentModel;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Core.PostInstall
{
    public class ServiceListModel : INotifyPropertyChanged
    {
        private readonly IRegistry _registry;
        private readonly IAppLogger _logger;
        private readonly IProcessRunner _processRunner;
        private readonly IServiceControllerFactory _serviceControllerFactory;
        private IEnumerable<ServiceModel> _services;


        public ServiceListModel(IRegistry registry, IAppLogger logger, IProcessRunner processRunner, IServiceControllerFactory serviceControllerFactory)
        {
            _logger = logger;
            _registry = registry;
            _processRunner = processRunner;
            _serviceControllerFactory = serviceControllerFactory;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public IEnumerable<ServiceModel> Services
        {
            get { return _services; }
            set
            {
                _services = value;
                OnPropertyChanged(nameof(Services));
            }
        }


        public void LoadOrRefresh()
        {
            Services = new List<ServiceModel>(GetAllRadeonServices());
        }

        public void ApplyChanges()
        {
            foreach (ServiceModel service in _services)
            {
                if (service.Enabled)
                    service.Enable();
                else
                    service.Disable();
            }
        }


        private IEnumerable<ServiceModel> GetAllRadeonServices()
        {
            string[] serviceNames =
            {
                //Values are the first string after AddService in inf files

                //Main display driver
                //Probably no point in showing this. Is there even a reason to remove it?
                //"amdkmdag",
                //"amdwddmg",
                //"amduw23g",

                //AMD PCI Root Bus Lower Filter
                //Probably shouldn't mess with this one either
                //"amdkmpfd",
                //"amdkmdap",

                //System Devices/Kernel Drivers
                "amdfendr",
                "amdfendrmgr",
                "amdlog",
                "AMDXE",

                //Audio
                "amdacpbus",
                "AMDAcpBtAudioService",
                "AMDAfdAudioService",
                "AMDHDAudBusService",
                "amdi2stdmafd",
                "AMDSoundWireAudioService",
                "AtiHDAudioService",
                "AMDSAFD",
                "amdacphpdsvc",
                "AMDAcpUsbAudioService",
                "AMDI2SAudioService",
                "AMDKSLFilterService",
                "amdsdwc",
                "amdsdws",

                //NT/Windows Services
                "AMD Crash Defender Service",
                "AMD External Events Utility",
                "AMD Log Utility",
                "AUEPLauncher",

                //Other
                "AMDRadeonsettings",
                "AMDFDANS",

                //Radeon Pro Enterprise
                "amducsi",
                "SSGService",

                // AMDISP
                "AmdCamera",
                "ispBridgeDevice",
                "sensorGC1029",
                "sensorHM1092",
                "sensorOV05C",
                "sensorOV08X",
                "sensorOV13B",
                "sensorVD55G0",

                // AMDNPUMCDM
                "IpuMcdmDriver",

                // AMDNPUWDF
                "kipudrv"
            };

            foreach (string service in serviceNames)
            {
                ServiceModel serviceModel = new ServiceModel(service, _registry, _logger, _processRunner, _serviceControllerFactory);

                if (serviceModel.Exists())
                {
                    _logger.Debug($"Found service {service}");
                    yield return serviceModel;
                }
            }
        }
    }
}
