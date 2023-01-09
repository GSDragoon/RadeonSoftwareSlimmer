using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using RadeonSoftwareSlimmer.Services;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Models.PreInstall
{
    public class InstallerFilesModel : INotifyPropertyChanged
    {
        private readonly IFileSystem _fileSystem;
        private string _installerFile;
        private string _extractedInstallerDirectory;


        public InstallerFilesModel(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public string InstallerFile
        {
            get { return _installerFile; }
            set
            {
                _installerFile = value;
                OnPropertyChanged(nameof(InstallerFile));
            }
        }
        public string ExtractedInstallerDirectory
        {
            get { return _extractedInstallerDirectory; }
            set
            {
                _extractedInstallerDirectory = value;
                OnPropertyChanged(nameof(ExtractedInstallerDirectory));
            }
        }


        public void ExtractInstallerFiles()
        {
            ProcessHandler processHandler = new ProcessHandler($@"{_fileSystem.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\7-Zip\7z.exe");
            int exitCode = processHandler.RunProcess($"x \"{InstallerFile}\" -o\"{ExtractedInstallerDirectory}\"");

            //https://sevenzip.osdn.jp/chm/cmdline/exit_codes.htm
            //Add messages for each possibility?
            if (exitCode == 7)
                throw new IOException("Extraction failed. 7-Zip error. See logging for more details.");
        }

        public void RunRadeonSoftwareSetup()
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = $@"{ExtractedInstallerDirectory}\Setup.exe";
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = false;
                process.Start();
            }
        }

        public void RunAmdCleanupUtility()
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = $@"{ExtractedInstallerDirectory}\Bin64\AMDCleanupUtility.exe";
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = false;
                process.Start();
            }
        }

        public bool ValidateInstallerFile()
        {
            if (string.IsNullOrWhiteSpace(_installerFile))
            {
                StaticViewModel.AddLogMessage($"Please provide an installer file");
                return false;
            }

            if (!_fileSystem.File.Exists(_installerFile))
            {
                StaticViewModel.AddLogMessage($"Installer file {_installerFile} does not exist or cannot be accessed");
                return false;
            }

            return true;
        }

        public bool ValidatePreExtractLocation()
        {
            if (string.IsNullOrWhiteSpace(_extractedInstallerDirectory))
            {
                StaticViewModel.AddLogMessage($"Please enter an extraction path");
                return false;
            }

            IDirectoryInfo directoryInfo = _fileSystem.DirectoryInfo.New(_extractedInstallerDirectory);

            if (directoryInfo.Exists && (directoryInfo.GetDirectories().Length > 0 || directoryInfo.GetFiles().Length > 0))
            {
                StaticViewModel.AddLogMessage($"Extraction folder {_extractedInstallerDirectory} is not empty");
                return false;
            }

            return true;
        }

        public bool ValidateExtractedLocation()
        {
            if (string.IsNullOrWhiteSpace(_extractedInstallerDirectory))
            {
                StaticViewModel.AddLogMessage($"Please enter an extraction path");
                return false;
            }

            IDirectoryInfo directoryInfo = _fileSystem.DirectoryInfo.New(_extractedInstallerDirectory);

            if (directoryInfo.Exists &&
                _fileSystem.Directory.Exists($"{_extractedInstallerDirectory}\\Bin64") &&
                _fileSystem.Directory.Exists($"{_extractedInstallerDirectory}\\Config") &&
                _fileSystem.File.Exists($"{_extractedInstallerDirectory}\\Setup.exe") &&
                _fileSystem.File.Exists($"{_extractedInstallerDirectory}\\Bin64\\AMDCleanupUtility.exe")
                )
                return true;
            else
            {
                StaticViewModel.AddLogMessage($"Installer files not found in {_extractedInstallerDirectory}");
                return false;
            }
        }
    }
}
