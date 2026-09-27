using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Core.PreInstall
{
    public class DisplayComponentListModel : INotifyPropertyChanged
    {
        // Combined driver packages may ship a second display driver alongside the primary one.
        private static readonly string[] DisplayFolderNames = { "Display", "Display2" };

        private readonly IAppLogger _logger;
        private readonly IFileSystem _fileSystem;
        private IDirectoryInfo _installDir;
        private IDirectoryInfo _backupBaseDir;
        private IEnumerable<DisplayComponentModel> _components;


        public DisplayComponentListModel(IFileSystem fileSystem, IAppLogger logger)
        {
            _logger = logger;
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
            foreach (IDirectoryInfo namespacedBackupDir in _backupBaseDir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                IDirectoryInfo componentBaseDir = GetComponentBaseDir(namespacedBackupDir.Name);
                componentBaseDir.Create();

                foreach (IDirectoryInfo backedUpComponentDir in namespacedBackupDir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    // If loading from another instance of the application, it won't have the old component information
                    // Move also changes the original directory object's path
                    _logger.Debug($"Restoring display component {backedUpComponentDir.Name} to {componentBaseDir.FullName}");
                    backedUpComponentDir.MoveTo(_fileSystem.Path.Combine(componentBaseDir.FullName, backedUpComponentDir.Name));
                }
            }
        }


        private IDirectoryInfo GetComponentBaseDir(string displayFolderName)
        {
            return _fileSystem.DirectoryInfo.New(_fileSystem.Path.Combine(_installDir.FullName, "Packages", "Drivers", displayFolderName, "WT6A_INF"));
        }

        private IEnumerable<DisplayComponentModel> GetDisplayComponents()
        {
            foreach (string displayFolderName in DisplayFolderNames)
            {
                IDirectoryInfo componentBaseDir = GetComponentBaseDir(displayFolderName);
                if (!componentBaseDir.Exists)
                    continue;

                IDirectoryInfo namespacedBackupDir = null;
                foreach (IDirectoryInfo componentDirectory in componentBaseDir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    if (componentDirectory.GetFiles("*.inf", SearchOption.TopDirectoryOnly).Length == 1)
                    {
                        if (namespacedBackupDir == null)
                            namespacedBackupDir = _backupBaseDir.CreateSubdirectory(displayFolderName);
                        yield return new DisplayComponentModel(_installDir, componentDirectory, namespacedBackupDir, _logger);
                    }
                }
            }
        }
    }
}
