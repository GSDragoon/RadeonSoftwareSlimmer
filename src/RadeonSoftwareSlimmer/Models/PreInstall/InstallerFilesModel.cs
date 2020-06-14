using System;
using System.ComponentModel;
using System.Diagnostics;
using RadeonSoftwareSlimmer.Services;

namespace RadeonSoftwareSlimmer.Models.PreInstall
{
    public class InstallerFilesModel : INotifyPropertyChanged
    {
        private string _installerFile;
        private string _extractedInstallerDirectory;


        public InstallerFilesModel() { }


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
            //TODO: Override existing files switch
            int exitCode = ProcessExecutor.RunProcess($@"{Environment.CurrentDirectory}\7-Zip\7z.exe", $"x \"{InstallerFile}\" -o\"{ExtractedInstallerDirectory}\"");

            //https://sevenzip.osdn.jp/chm/cmdline/exit_codes.htm
            //Add messages for each possibility?
            if (exitCode == 7)
                throw new Exception("Extraction failed. 7-Zip Command line error");
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
    }
}
