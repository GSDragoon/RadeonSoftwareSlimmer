using System;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Models.PreInstall
{
    public class DisplayComponentModel : INotifyPropertyChanged
    {
        private readonly IDirectoryInfo _componentDirectory;
        private readonly IDirectoryInfo _backupDirectory;
        private bool _keep;


        public DisplayComponentModel(IDirectoryInfo installerRootDirectory, IDirectoryInfo componentDirectory)
        {
            StaticViewModel.AddDebugMessage($"Found display component in {componentDirectory.FullName}");

            Keep = true;

            _componentDirectory = componentDirectory;
            _backupDirectory = installerRootDirectory.CreateSubdirectory("RSS_Backup").CreateSubdirectory("DisplayComponents");
            Directory = componentDirectory.FullName.Substring(componentDirectory.FullName.IndexOf(installerRootDirectory.FullName) + installerRootDirectory.FullName.Length);

            IFileInfo infFile = componentDirectory.GetFiles("*.inf", SearchOption.TopDirectoryOnly)[0];
            InfFile = infFile.Name;
            LoadInfFileInformation(infFile);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public bool Keep
        {
            get { return _keep; }
            set
            {
                _keep = value;
                OnPropertyChanged(nameof(Keep));
            }
        }
        public string Directory { get; }
        public string InfFile { get; }
        public string Description { get; private set; }


        public void Remove()
        {
            if (Keep || !_componentDirectory.Exists)
                return;

            StaticViewModel.AddDebugMessage($"Removing {_componentDirectory.FullName}");

            foreach (IFileInfo file in _componentDirectory.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                if (file.IsReadOnly)
                {
                    file.IsReadOnly = false;
                }
            }

            string componentBackupDir = _backupDirectory.FileSystem.Path.Combine(_backupDirectory.FullName, _componentDirectory.Name);
            StaticViewModel.AddDebugMessage($"Moving display component {_componentDirectory.Name} to backup path {componentBackupDir}");
            _componentDirectory.MoveTo(componentBackupDir);
        }


#if NET5_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2249:Consider using 'string.Contains' instead of 'string.IndexOf'", Justification = "Cannot do case-insensitive Contains in .NET 4.8")]
#endif
        private void LoadInfFileInformation(IFileInfo infFile)
        {
            StaticViewModel.AddDebugMessage($"Processing inf file {infFile.FullName}");

            using (StreamReader reader = new StreamReader(infFile.OpenRead()))
            {
                string line = string.Empty;
                do
                {
                    line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line) && line.Equals("[Strings]", StringComparison.Ordinal))
                    {
                        while (!reader.EndOfStream && !string.IsNullOrWhiteSpace(line))
                        {
                            line = reader.ReadLine();

                            //Would love some consistency here
                            if ((line.IndexOf("desc", StringComparison.OrdinalIgnoreCase) >= 0 && line.IndexOf("\"", StringComparison.OrdinalIgnoreCase) > 1) ||
                                (line.IndexOf("ExtendedGraphics", StringComparison.OrdinalIgnoreCase) >= 0 && line.IndexOf("\"", StringComparison.OrdinalIgnoreCase) > 1) ||
                                line.StartsWith("AMDFDANSName", StringComparison.OrdinalIgnoreCase) ||
                                line.StartsWith("AMDOCLName", StringComparison.OrdinalIgnoreCase) || 
                                line.StartsWith("AMDWINName", StringComparison.OrdinalIgnoreCase))
                            {
                                StaticViewModel.AddDebugMessage($"Attempting to obtain inf file description from {line}");
                                Description = line.Substring(line.IndexOf("\"", StringComparison.OrdinalIgnoreCase)).Trim('\"');
                                return;
                            }
                        }
                    }

                } while (!reader.EndOfStream);
            }
        }
    }
}
