using System.ComponentModel;
using System.IO.Abstractions;

namespace RadeonSoftwareSlimmer.Models.PreInstall
{
    public class PackageModel : INotifyPropertyChanged
    {
        private readonly IFileInfo _file;
        private bool _keep;

        
        public PackageModel(IFileInfo file)
        {
            _file = file;
            Keep = true;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public bool Keep
        {
            get { return _keep; }
            set
            {
                _keep = value;
                OnPropertyChanged(nameof(Keep));
            }
        }
        public string ProductName { get; internal set; }
        public string Url { get; internal set; }
        public string Type { get; internal set; }
        public string Description { get; internal set; }


        public IFileInfo GetFile()
        {
            return _file;
        }

        public bool Equals(PackageModel package)
        {
            if (package == null)
                return false;

            return GetComparisonHashCode().Equals(package.GetComparisonHashCode());
        }


        private int GetComparisonHashCode()
        {
            return $"{ProductName}|{Url}|{Description}|{Type}|{_file.FullName}".GetHashCode();
        }
    }
}
