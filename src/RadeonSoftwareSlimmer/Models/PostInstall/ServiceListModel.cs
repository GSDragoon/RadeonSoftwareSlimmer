using System.Collections.Generic;
using System.ComponentModel;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Models.PostInstall
{
    public class ServiceListModel : INotifyPropertyChanged
    {
        private IEnumerable<ServiceModel> _services;


        public ServiceListModel() { }


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


        private static IEnumerable<ServiceModel> GetAllRadeonServices()
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
                "amdlog",
                "AMDXE",

                //Audio
                "amdacpbus",
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
                ServiceModel serviceModel = new ServiceModel(service);

                if (serviceModel.Exists())
                {
                    StaticViewModel.AddDebugMessage($"Found service {service}");
                    yield return serviceModel;
                }
            }
        }
    }
}
