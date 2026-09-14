using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Abstractions;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Core.PostInstall
{
    public class TempFileListModel : INotifyPropertyChanged
    {
        private readonly IFileSystem _fileSystem;
        private readonly IAppLogger _logger;
        private IEnumerable<TempFileModel> _tempFiles;


        public TempFileListModel(IFileSystem fileSystem, IAppLogger logger)
        {
            _logger = logger;
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
            string systemDrive = _fileSystem.Path.GetPathRoot(Environment.SystemDirectory) ?? string.Empty; // null on linux for local testing
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolderOption.DoNotVerify);
            string appDataLocal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify);
            string appDataLocalLow = _fileSystem.Path.Combine(userProfile, "AppData", "LocalLow");

            string[] tempFolders =
            {
                //C:\AMD
                _fileSystem.Path.Combine(systemDrive, "AMD"),

                _fileSystem.Path.Combine(programFiles, "AMD", "AMDInstallManager", "Logs"),
                //Computer\HKEY_LOCAL_MACHINE\SOFTWARE\ATI Technologies\Install,InstallDir,C:\Program Files\AMD\CIM
                _fileSystem.Path.Combine(programFiles, "AMD", "CIM", "Log"),
                _fileSystem.Path.Combine(programFiles, "AMD", "CIM", "Reports"),

                _fileSystem.Path.Combine(appDataLocal, "AMD_Common"),
                _fileSystem.Path.Combine(appDataLocal, "AMDIdentifyWindow", "cache"),
                _fileSystem.Path.Combine(appDataLocal, "AMDInstallManager", "cache"),
                _fileSystem.Path.Combine(appDataLocal, "AMDSoftwareInstaller", "cache"),
                _fileSystem.Path.Combine(appDataLocal, "RadeonInstaller", "cache"),

                _fileSystem.Path.Combine(appDataLocal, "AMD", "AMDLink", "cache"),
                _fileSystem.Path.Combine(appDataLocal, "AMD", "CN", "Analytics"),
                _fileSystem.Path.Combine(appDataLocal, "AMD", "CN", "NewsFeed"),
                _fileSystem.Path.Combine(appDataLocal, "AMD", "LINK", "game_cache"),
                _fileSystem.Path.Combine(appDataLocal, "AMD", "Radeonsoftware", "cache"),

                _fileSystem.Path.Combine(appDataLocal, "AMD", "cl.cache"),
                _fileSystem.Path.Combine(appDataLocal, "AMD", "Dx9Cache"),
                _fileSystem.Path.Combine(appDataLocal, "AMD", "DxCache"),
                _fileSystem.Path.Combine(appDataLocal, "AMD", "DxcCache"),
                _fileSystem.Path.Combine(appDataLocal, "AMD", "GLCache"),
                _fileSystem.Path.Combine(appDataLocal, "AMD", "oglcache"),
                _fileSystem.Path.Combine(appDataLocal, "AMD", "OglpCache"),
                _fileSystem.Path.Combine(appDataLocal, "AMD", "VkCache"),

                _fileSystem.Path.Combine(appDataLocalLow, "AMD", "cl.cache"),
                _fileSystem.Path.Combine(appDataLocalLow, "AMD", "Dx9Cache"),
                _fileSystem.Path.Combine(appDataLocalLow, "AMD", "DxCache"),
                _fileSystem.Path.Combine(appDataLocalLow, "AMD", "DxcCache"),
                _fileSystem.Path.Combine(appDataLocalLow, "AMD", "GLCache"),
                _fileSystem.Path.Combine(appDataLocalLow, "AMD", "oglcache"),
                _fileSystem.Path.Combine(appDataLocalLow, "AMD", "OglpCache"),
                _fileSystem.Path.Combine(appDataLocalLow, "AMD", "VkCache"),

                //C:\Windows\System32\AMD
                _fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System, Environment.SpecialFolderOption.DoNotVerify), "AMD", "EeuDumps"),
                _fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System, Environment.SpecialFolderOption.DoNotVerify), "AMD", "MmdDumps"),
            };

            foreach (string tempFolder in tempFolders)
            {
                if (_fileSystem.Directory.Exists(tempFolder))
                {
                    _logger.Debug($"Found temp folder {tempFolder}");
                    yield return new TempFileModel(tempFolder, _fileSystem, _logger);
                }
                else
                {
                    _logger.Debug($"Folder {tempFolder} does not exist or cannot be accessed");
                }
            }
        }
    }
}
