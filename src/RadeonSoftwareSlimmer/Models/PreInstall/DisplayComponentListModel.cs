using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;

namespace RadeonSoftwareSlimmer.Models.PreInstall
{
    public class DisplayComponentListModel : INotifyPropertyChanged
    {
        private readonly IFileSystem _fileSystem;
        private IEnumerable<DisplayComponentModel> _components;
        private readonly string[] _displayDriverFolders = new string[5]{string.Empty, "Packages", "Drivers", "Display", "WT6A_INF" };

        public DisplayComponentListModel(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public IEnumerable<DisplayComponentModel> DisplayDriverComponents
        {
            get { return _components; }
            set
            {
                _components = value;
                OnPropertyChanged(nameof(DisplayDriverComponents));
            }
        }


        public void LoadOrRefresh(string installDirectory)
        {
            DisplayDriverComponents = new List<DisplayComponentModel>(GetDisplayComponentFolders(installDirectory));
        }

        public void RemoveComponentsNotKeeping()
        {
            foreach (DisplayComponentModel displayComponentModel in _components)
            {
                if (!displayComponentModel.Keep)
                {
                    displayComponentModel.Remove();
                }
            }
        }


        private IEnumerable<DisplayComponentModel> GetDisplayComponentFolders(string installerRoot)
        {
            IDirectoryInfo installerRootDirectory = _fileSystem.DirectoryInfo.New(installerRoot);
            if (!installerRootDirectory.Exists)
                throw new DirectoryNotFoundException("Installer folder does not exist or cannot access.");

            _displayDriverFolders[0] = installerRoot;
            IDirectoryInfo displayDriverFolder = _fileSystem.DirectoryInfo.New(_fileSystem.Path.Combine(_displayDriverFolders));
            if (displayDriverFolder.Exists)
            {
                foreach (IDirectoryInfo componentDirectory in displayDriverFolder.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    if (componentDirectory.GetFiles("*.inf", SearchOption.TopDirectoryOnly).Length == 1)
                        yield return new DisplayComponentModel(installerRootDirectory, componentDirectory);
                }
            }
        }
    }
}
