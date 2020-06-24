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


        private static IEnumerable<ServiceModel> GetAllRadeonServices()
        {
            //If AMD was consistent with anything, this could be nicer
            string[] serviceNames =
            {
                //System Devices
                "amdlog",
                "AMDXE",

                //NT Services
                "AMD Crash Defender Service",
                "AMD External Events Utility",
                "AMD Log Utility",
                "AUEPLauncher",
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
