using System.Collections.Generic;
using System.ComponentModel;
using RadeonSoftwareSlimmer.Intefaces;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Models.PostInstall
{
    public class ServiceListModel : INotifyPropertyChanged
    {
        private readonly IRegistry _registry;
        private IEnumerable<ServiceModel> _services;


        public ServiceListModel(IRegistry registry)
        {
            _registry = registry;
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

                //AMD PCI Root Bus Lower Filter
                //Probably shouldn't mess with this one either
                //"amdkmpfd",

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

                //NT/Windows Services
                "AMD Crash Defender Service",
                "AMD External Events Utility",
                "AMD Log Utility",
                "AUEPLauncher",

                //Other
                "AMDRadeonsettings",

                //Radeon Pro Enterprise
                "amducsi",
                "SSGService",
            };

            foreach (string service in serviceNames)
            {
                ServiceModel serviceModel = new ServiceModel(service, _registry);

                if (serviceModel.Exists())
                {
                    StaticViewModel.AddDebugMessage($"Found service {service}");
                    yield return serviceModel;
                }
            }
        }
    }
}
