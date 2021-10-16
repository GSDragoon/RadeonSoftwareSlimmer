using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Abstractions;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Models.PostInstall
{
    public class TempFileListModel : INotifyPropertyChanged
    {
        private readonly IFileSystem _fileSystem;
        private IEnumerable<TempFileModel> _tempFiles;


        public TempFileListModel(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public IEnumerable<TempFileModel> TempFiles
        {
            get { return _tempFiles; }
            set
            {
                _tempFiles = value;
                OnPropertyChanged(nameof(TempFiles));
            }
        }


        public void LoadOrRefresh()
        {
            TempFiles = new List<TempFileModel>(GetAllRadeonTempFiles());
        }

        public void ApplyChanges()
        {
            foreach (TempFileModel tempFile in _tempFiles)
            {
                if (tempFile.Clear)
                    tempFile.ClearFolder();
            }
        }


        private IEnumerable<TempFileModel> GetAllRadeonTempFiles()
        {
            string[] tempFolders =
            {
                //C:\AMD
                $@"{Environment.GetEnvironmentVariable("SystemDrive", EnvironmentVariableTarget.Process)}\AMD",

                //Computer\HKEY_LOCAL_MACHINE\SOFTWARE\ATI Technologies\Install,InstallDir,C:\Program Files\AMD\CIM
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolderOption.DoNotVerify)}\AMD\CIM\Log",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolderOption.DoNotVerify)}\AMD\CIM\Reports",

                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD\AMDLink\cache",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD\CN\Analytics",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD\CN\NewsFeed",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD_Common",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMDIdentifyWindow\cache",

                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD\Dx9Cache",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD\DxCache",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD\GLCache",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD\VkCache",

                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD\Radeonsoftware\cache",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\RadeonInstaller\cache",

                $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify)}\AppData\LocalLow\AMD\Dx9Cache",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify)}\AppData\LocalLow\AMD\DxCache",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify)}\AppData\LocalLow\AMD\GLCache",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify)}\AppData\LocalLow\AMD\VkCache",

                //C:\Windows\System32\AMD
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.System, Environment.SpecialFolderOption.DoNotVerify)}\AMD",
            };

            foreach (string tempFolder in tempFolders)
            {
                if (_fileSystem.Directory.Exists(tempFolder))
                {
                    StaticViewModel.AddDebugMessage($"Found temp folder {tempFolder}");
                    yield return new TempFileModel(tempFolder, _fileSystem);
                }
                else
                {
                    StaticViewModel.AddDebugMessage($"Folder {tempFolder} does not exist or cannot be accessed");
                }
            }
        }
    }
}
