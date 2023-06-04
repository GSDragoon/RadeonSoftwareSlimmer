using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
#if NET48
using System.Linq;
#endif
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
#if NET6_0_OR_GREATER
        // .NET 6+ removed certain characters for reasons I don't like
        // https://github.com/dotnet/runtime/issues/63383
        // https://github.com/dotnet/corefx/pull/8669/files#r63910570
        private readonly char[] extraInvalidDirChars = { '\"', '<', '>', };
#endif


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
            string sevenZipExe = _fileSystem.Path.Combine(_fileSystem.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "7-Zip", "7z.exe");
            ProcessHandler processHandler = new ProcessHandler(sevenZipExe);
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
                process.StartInfo.FileName = _fileSystem.Path.Combine(ExtractedInstallerDirectory, "Setup.exe");
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = false;
                process.Start();
            }
        }

        public void RunAmdCleanupUtility()
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = _fileSystem.Path.Combine(ExtractedInstallerDirectory, "Bin64", "AMDCleanupUtility.exe");
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = false;
                process.Start();
            }
        }

        public bool ValidateInstallerFile()
        {
            if (string.IsNullOrWhiteSpace(_installerFile))
            {
                StaticViewModel.AddLogMessage("Please provide an installer file");
                return false;
            }

            try
            {
                IFileInfo fileInfo = _fileSystem.FileInfo.New(_installerFile);

                if (Array.Exists(_fileSystem.Path.GetInvalidPathChars(), c => fileInfo.DirectoryName.Contains(c)))
                {
                    StaticViewModel.AddLogMessage("File directory contains invalid characters");
                    return false;
                }
                if (Array.Exists(_fileSystem.Path.GetInvalidFileNameChars(), (c => fileInfo.Name.Contains(c))))
                {
                    StaticViewModel.AddLogMessage("File name contains invalid characters");
                    return false;
                }
                if (!fileInfo.Exists)
                {
                    StaticViewModel.AddLogMessage($"Installer file {_installerFile} does not exist or cannot be accessed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // FileInfo.New validates the directory path
                StaticViewModel.AddLogMessage(ex);
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
            try
            {
                IDirectoryInfo directoryInfo = _fileSystem.DirectoryInfo.New(_extractedInstallerDirectory);

                if (Array.Exists(_fileSystem.Path.GetInvalidPathChars(), c => directoryInfo.FullName.Contains(c)))
                {
                    StaticViewModel.AddLogMessage("Directory contains invalid characters");
                    return false;
                }
#if NET6_0_OR_GREATER

                if (Array.Exists(extraInvalidDirChars, c => directoryInfo.FullName.Contains(c)))
                {
                    StaticViewModel.AddLogMessage("Directory contains invalid characters");
                    return false;
                }
#endif

                if (directoryInfo.Exists && (directoryInfo.GetDirectories().Length > 0 || directoryInfo.GetFiles().Length > 0))
                {
                    StaticViewModel.AddLogMessage($"Extraction folder {_extractedInstallerDirectory} is not empty");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // DirectoryInfo.New validates the path
                StaticViewModel.AddLogMessage(ex);
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

            try
            {
                IDirectoryInfo directoryInfo = _fileSystem.DirectoryInfo.New(_extractedInstallerDirectory);

                if (Array.Exists(_fileSystem.Path.GetInvalidPathChars(), c => directoryInfo.FullName.Contains(c)))
                {
                    StaticViewModel.AddLogMessage("Directory contains invalid characters");
                    return false;
                }
#if NET6_0_OR_GREATER

                if (Array.Exists(extraInvalidDirChars, c => directoryInfo.FullName.Contains(c)))
                {
                    StaticViewModel.AddLogMessage("Directory contains invalid characters");
                    return false;
                }
#endif

                if (directoryInfo.Exists &&
                    _fileSystem.Directory.Exists(_fileSystem.Path.Combine(_extractedInstallerDirectory, "Bin64")) &&
                    _fileSystem.Directory.Exists(_fileSystem.Path.Combine(_extractedInstallerDirectory, "Config")) &&
                    _fileSystem.File.Exists(_fileSystem.Path.Combine(_extractedInstallerDirectory, "Setup.exe")) &&
                    _fileSystem.File.Exists(_fileSystem.Path.Combine(_extractedInstallerDirectory, "Bin64", "AMDCleanupUtility.exe")))
                {
                    return true;
                }
                else
                {
                    StaticViewModel.AddLogMessage($"Expected installer files not found in {_extractedInstallerDirectory}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // DirectoryInfo.New validates the path
                StaticViewModel.AddLogMessage(ex);
                return false;
            }
        }
    }
}
