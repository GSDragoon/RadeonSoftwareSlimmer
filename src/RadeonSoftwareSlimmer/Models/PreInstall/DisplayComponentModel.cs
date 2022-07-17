using System;
using System.IO;
using System.IO.Abstractions;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Models.PreInstall
{
    public class DisplayComponentModel
    {
        private readonly IDirectoryInfo _componentDirectory;


        public DisplayComponentModel(IDirectoryInfo installerRootDirectory, IDirectoryInfo componentDirectory)
        {
            StaticViewModel.AddDebugMessage($"Found display component in {componentDirectory.FullName}");

            Keep = true;

            _componentDirectory = componentDirectory;
            Directory = componentDirectory.FullName.Substring(componentDirectory.FullName.IndexOf(installerRootDirectory.FullName) + installerRootDirectory.FullName.Length);
            IFileInfo[] infFiles = componentDirectory.GetFiles("*.inf", SearchOption.TopDirectoryOnly);
            if (infFiles.Length > 0)
            {
                InfFile = infFiles[0].Name;
                LoadInfFileInformation(infFiles[0]);
            }
        }


        public bool Keep { get; set; }
        public string Directory { get; }
        public string InfFile { get; }
        public string Description { get; private set; }


        public void Remove()
        {
            if (Keep || !_componentDirectory.Exists)
                return;

            try
            {
                StaticViewModel.AddDebugMessage($"Removing {_componentDirectory.FullName}");

                foreach (IFileInfo file in _componentDirectory.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    if (file.IsReadOnly)
                    {
                        file.IsReadOnly = false;
                    }
                }

                _componentDirectory.Delete(true);
            }
            catch (Exception ex)
            {
                StaticViewModel.AddDebugMessage(ex, $"Unable to delete {_componentDirectory.FullName}");
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2249:Consider using 'string.Contains' instead of 'string.IndexOf'", Justification = "Cannot do case-insensitive Contains in .NET 4.8")]
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
                                (line.StartsWith("AMDFDANSName", StringComparison.OrdinalIgnoreCase))
                                )
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
