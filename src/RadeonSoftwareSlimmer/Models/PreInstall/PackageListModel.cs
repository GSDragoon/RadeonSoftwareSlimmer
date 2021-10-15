using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Models.PreInstall
{
    public class PackageListModel : INotifyPropertyChanged
    {
        private readonly IFileSystem _fileSystem;
        private IEnumerable<PackageModel> _packages;


        public PackageListModel(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
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
                InstallerPackages = new List<PackageModel>(GetAllInstallerPackages(installDirectory).OrderBy(p => p.ProductName));
        }

        public static void RemovePackage(PackageModel packageToRemove)
        {
            if (packageToRemove == null)
                throw new ArgumentNullException(nameof(packageToRemove));

            JObject fullJson;

            StaticViewModel.AddDebugMessage($"Removing package {packageToRemove.ProductName} from {packageToRemove.GetFile().FullName}");


            using (StreamReader streamReader = new StreamReader(packageToRemove.GetFile().OpenRead()))
            using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
            {
                fullJson = (JObject)JToken.ReadFrom(jsonTextReader);
                JToken jToken = fullJson.SelectToken("Packages.Package");
                foreach (JToken token in jToken.Children())
                {
                    PackageModel currentPackage = new PackageModel(packageToRemove.GetFile());
                    currentPackage.Description = token.SelectToken("Info.Description").ToString();
                    currentPackage.ProductName = token.SelectToken("Info.productName").ToString();
                    currentPackage.Url = token.SelectToken("Info.url").ToString();
                    currentPackage.Type = token.SelectToken("Info.ptype").ToString();

                    if (currentPackage.Equals(packageToRemove))
                    {
                        token.Remove();
                        break;
                    }
                }
            }

            using (StreamWriter streamWriter = new StreamWriter(packageToRemove.GetFile().Open(FileMode.Create, FileAccess.Write, FileShare.None)))
            using (JsonTextWriter jsonTextWriter = new JsonTextWriter(streamWriter))
            {
                jsonTextWriter.Formatting = Formatting.Indented;
                fullJson.WriteTo(jsonTextWriter);
            }
        }


        private IEnumerable<PackageModel> GetAllInstallerPackages(IDirectoryInfo installDirectory)
        {
            IFileInfo[] packageFiles =
            {
                _fileSystem.FileInfo.FromFileName($@"{installDirectory.FullName}\Bin64\cccmanifest_64.json"),
                _fileSystem.FileInfo.FromFileName($@"{installDirectory.FullName}\Config\InstallManifest.json"),
            };

            foreach (IFileInfo file in packageFiles)
            {
                if (file.Exists)
                {
                    using (StreamReader streamReader = new StreamReader(file.OpenRead()))
                    using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                    {
                        JObject jObject = (JObject)JToken.ReadFrom(jsonTextReader);
                        JToken jToken = jObject.SelectToken("Packages.Package");
                        foreach (JToken token in jToken.Children())
                        {
                            PackageModel package = new PackageModel(file);
                            package.Description = token.SelectToken("Info.Description").ToString();
                            package.ProductName = token.SelectToken("Info.productName").ToString();
                            package.Url = token.SelectToken("Info.url").ToString();
                            package.Type = token.SelectToken("Info.ptype").ToString();

                            StaticViewModel.AddDebugMessage($"Found package {package.ProductName} in {package.GetFile().FullName}");
                            yield return package;
                        }
                    }
                }
            }
        }
    }
}
