using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Core.PreInstall
{
    public class PackageListModel : INotifyPropertyChanged
    {
        private readonly IAppLogger _logger;
        private readonly IFileSystem _fileSystem;
        private readonly string[] _packageFiles;
        private IEnumerable<PackageModel> _packages;
        private IDirectoryInfo _installDir;
        private IDirectoryInfo _backupDir;


        public PackageListModel(IFileSystem fileSystem, IAppLogger logger)
        {
            _logger = logger;
            _fileSystem = fileSystem;
            _packageFiles = new[]
            {
                _fileSystem.Path.Combine("Bin64", "cccmanifest_64.json"),
                _fileSystem.Path.Combine("Config", "InstallManifest.json"),
            };
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public IEnumerable<PackageModel> InstallerPackages
        {
            get { return _packages; }
            set
            {
                _packages = value;
                OnPropertyChanged(nameof(InstallerPackages));
            }
        }


        public void LoadOrRefresh(IDirectoryInfo installDirectory)
        {
            if (installDirectory != null)
            {
                _installDir = installDirectory;
                _backupDir = installDirectory.CreateSubdirectory("RSS_Backup").CreateSubdirectory("Packages");
                BackupIfNotAlready();

                InstallerPackages = new List<PackageModel>(GetAllInstallerPackages().OrderBy(p => p.ProductName));
            }
        }

        public void RemovePackage(PackageModel packageToRemove)
        {
            if (packageToRemove == null)
                throw new ArgumentNullException(nameof(packageToRemove));

            _logger.Debug($"Removing package {packageToRemove.ProductName} from {packageToRemove.GetFile().FullName}");

            JsonNode fullJson;
            using (Stream stream = packageToRemove.GetFile().OpenRead())
            {
                fullJson = JsonNode.Parse(stream);
            }

            JsonArray packageArray = (JsonArray)fullJson["Packages"]["Package"];
            for (int i = 0; i < packageArray.Count; i++)
            {
                JsonNode token = packageArray[i];
                PackageModel currentPackage = new PackageModel(packageToRemove.GetFile())
                {
                    Description = (string)token["Info"]["Description"],
                    ProductName = (string)token["Info"]["productName"],
                    Url = (string)token["Info"]["url"],
                    Type = (string)token["Info"]["ptype"],
                };

                if (currentPackage.Equals(packageToRemove))
                {
                    packageArray.RemoveAt(i);
                    break;
                }
            }

            using (Stream stream = packageToRemove.GetFile().Open(FileMode.Create, FileAccess.Write, FileShare.None))
            using (Utf8JsonWriter writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
            {
                fullJson.WriteTo(writer);
            }
        }

        public void RestoreToDefault()
        {
            if (_backupDir.Exists)
            {
                foreach (string packageFile in _packageFiles)
                {
                    IFileInfo backupFile = _fileSystem.FileInfo.New(_fileSystem.Path.Combine(_backupDir.FullName, packageFile));
                    if (backupFile.Exists)
                    {
                        IFileInfo destinationFile = _fileSystem.FileInfo.New(_fileSystem.Path.Combine(_installDir.FullName, packageFile));
                        _fileSystem.File.Copy(backupFile.FullName, destinationFile.FullName, true);
                    }
                    else
                    {
                        _logger.Debug($"Attempted to restore package file {backupFile.FullName} from default, but no backup directory found.");
                    }
                }
            }
            else
            {
                _logger.Debug("Attempted to restore packages from default, but no backup directory found.");
            }
        }


        private IEnumerable<PackageModel> GetAllInstallerPackages()
        {
            foreach (string packageFile in _packageFiles)
            {
                IFileInfo file = _fileSystem.FileInfo.New(_fileSystem.Path.Combine(_installDir.FullName, packageFile));
                if (file.Exists)
                {
                    JsonNode fullJson;
                    using (Stream stream = file.OpenRead())
                    {
                        fullJson = JsonNode.Parse(stream);
                    }

                    JsonArray packageArray = (JsonArray)fullJson["Packages"]["Package"];
                    foreach (JsonNode token in packageArray)
                    {
                        PackageModel package = new PackageModel(file)
                        {
                            Description = (string)token["Info"]["Description"],
                            ProductName = (string)token["Info"]["productName"],
                            Url = (string)token["Info"]["url"],
                            Type = (string)token["Info"]["ptype"],
                        };

                        _logger.Debug($"Found package {package.ProductName} in {package.GetFile().FullName}");
                        yield return package;
                    }
                }
            }
        }

        private void BackupIfNotAlready()
        {
            foreach (string packageFile in _packageFiles)
            {
                IFileInfo backupFile = _fileSystem.FileInfo.New(_fileSystem.Path.Combine(_backupDir.FullName, packageFile));
                if (!backupFile.Exists)
                {
                    if (!_fileSystem.Directory.Exists(backupFile.DirectoryName))
                        _fileSystem.Directory.CreateDirectory(backupFile.DirectoryName);

                    IFileInfo sourceFile = _fileSystem.FileInfo.New(_fileSystem.Path.Combine(_installDir.FullName, packageFile));
                    _logger.Debug($"Backing up {sourceFile.FullName} to {backupFile.FullName}");
                    _fileSystem.File.Copy(sourceFile.FullName, backupFile.FullName);
                }
            }
        }
    }
}
