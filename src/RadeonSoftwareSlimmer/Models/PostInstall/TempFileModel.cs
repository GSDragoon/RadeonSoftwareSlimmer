using System;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Models.PostInstall
{
    public class TempFileModel : INotifyPropertyChanged
    {
        private readonly IFileSystem _fileSystem;

        //1024 instead of 1000, since this isn't a disk drive manufacturer...
        private const float DIV = 1024.0F;
        private bool _delete;


        public TempFileModel(string folder, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            Clear = false;
            Folder = folder;

            CalculateSize();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public bool Clear
        {
            get { return _delete; }
            set
            {
                _delete = value;
                OnPropertyChanged(nameof(Clear));
            }
        }
        public string Folder { get; }
        public int Files { get; private set; }
        public string Size { get; private set; }


        public void ClearFolder()
        {
            StaticViewModel.AddLogMessage($"Clearing folder {Folder}");
            ClearFolder(Folder);
        }


        private void CalculateSize()
        {
            long bytes = 0;
            int files = 0;
            IDirectoryInfo directoryInfo = _fileSystem.DirectoryInfo.FromDirectoryName(Folder);

            foreach (IFileInfo fileInfo in directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                files++;

                try
                {
                    bytes += fileInfo.Length;
                }
                catch (Exception ex)
                {
                    StaticViewModel.AddDebugMessage(ex, $"Unable to determine file size of {fileInfo.FullName}");
                }
            }

            Files = files;
            Size = GetFriendlySize(bytes);
        }

        private static string GetFriendlySize(long bytes)
        {
            if (bytes < DIV)
                return $"{bytes} bytes";

            if (bytes < (DIV * DIV))
                return $"{bytes / DIV:N2} KB";

            if (bytes < (DIV * DIV * DIV))
                return $"{bytes / (DIV * DIV):N2} MB";

            //Yikes!
            return $"{bytes / (DIV * DIV * DIV):N2} GB";
        }

        private void ClearFolder(string folder)
        {
            IDirectoryInfo directoryInfo = _fileSystem.DirectoryInfo.FromDirectoryName(folder);

            foreach (IFileInfo fileInfo in directoryInfo.EnumerateFiles())
            {
                try
                {
                    fileInfo.Delete();
                }
                catch (Exception ex)
                {
                    StaticViewModel.AddDebugMessage(ex, $"Unable to delete {fileInfo.FullName}");
                }
            }

            foreach (IDirectoryInfo subDirectoryInfo in directoryInfo.EnumerateDirectories())
            {
                ClearFolder(subDirectoryInfo.FullName);

                try
                {
                    subDirectoryInfo.Delete();
                }
                catch (Exception ex)
                {
                    StaticViewModel.AddDebugMessage(ex, $"Unable to delete {subDirectoryInfo.FullName}");
                }
            }
        }
    }
}
