using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Models.PostInstall
{
    public class TempFileListModel : INotifyPropertyChanged
    {
        private IEnumerable<TempFileModel> _tempFiles;


        public TempFileListModel() { }


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


        private static IEnumerable<TempFileModel> GetAllRadeonTempFiles()
        {
            string[] tempFolders =
            {
                //C:\AMD
                $@"{Environment.GetEnvironmentVariable("SystemDrive", EnvironmentVariableTarget.Process)}\AMD",

                //Computer\HKEY_LOCAL_MACHINE\SOFTWARE\ATI Technologies\Install,InstallDir,C:\Program Files\AMD\CIM
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolderOption.DoNotVerify)}\AMD\CIM\Log",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolderOption.DoNotVerify)}\AMD\CIM\Reports",

                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD\DxCache",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD\GLCache",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD\VkCache",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\AMD\Radeonsoftware\cache",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\RadeonInstaller\cache",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\LocalLow\AMD\DxCache",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\LocalLow\AMD\GLCache",
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify)}\LocalLow\AMD\VkCache",

                //C:\Windows\System32\AMD
                $@"{Environment.GetFolderPath(Environment.SpecialFolder.System, Environment.SpecialFolderOption.DoNotVerify)}\AMD",
            };

            foreach (string tempFolder in tempFolders)
            {
                if (Directory.Exists(tempFolder))
                {
                    StaticViewModel.AddDebugMessage($"Found temp folder {tempFolder}");
                    yield return new TempFileModel(tempFolder);
                }
                else
                {
                    StaticViewModel.AddDebugMessage($"Folder {tempFolder} does not exist or cannot be accessed");
                }
            }
        }
    }
}
