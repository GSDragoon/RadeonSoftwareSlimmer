using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Models.PreInstall
{
    public class DisplayComponentListModel : INotifyPropertyChanged
    {
        private readonly IFileSystem _fileSystem;
        private IDirectoryInfo _installDir;
        private IDirectoryInfo _componentBaseDir;
        private IDirectoryInfo _backupBaseDir;
        private IEnumerable<DisplayComponentModel> _components;


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


        public void LoadOrRefresh(IDirectoryInfo installDirectory)
        {
            _installDir = installDirectory;

            if (!_installDir.Exists)
                throw new DirectoryNotFoundException("Installer folder does not exist or cannot access.");

            _componentBaseDir = _fileSystem.DirectoryInfo.New(_fileSystem.Path.Combine(_installDir.FullName, "Packages", "Drivers", "Display", "WT6A_INF"));
            _backupBaseDir = _installDir.CreateSubdirectory("RSS_Backup").CreateSubdirectory("DisplayComponents");

            DisplayDriverComponents = new List<DisplayComponentModel>(GetDisplayComponents());
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

        public void RestoreToDefault()
        {
            foreach (IDirectoryInfo backedUpComponentDir in _backupBaseDir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                // If loading from another instance of the application, it won't have the old component information
                // Move also changes the original directory object's path
                StaticViewModel.AddDebugMessage($"Restoring display component {backedUpComponentDir.Name} to {_componentBaseDir.FullName}");
                backedUpComponentDir.MoveTo(_fileSystem.Path.Combine(_componentBaseDir.FullName, backedUpComponentDir.Name));
            }
        }


        private IEnumerable<DisplayComponentModel> GetDisplayComponents()
        {
            if (_componentBaseDir.Exists)
            {
                foreach (IDirectoryInfo componentDirectory in _componentBaseDir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    if (componentDirectory.GetFiles("*.inf", SearchOption.TopDirectoryOnly).Length == 1)
                        yield return new DisplayComponentModel(_installDir, componentDirectory);
                }
            }
        }
    }
}
