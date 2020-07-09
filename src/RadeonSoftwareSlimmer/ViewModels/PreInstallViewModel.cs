using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using RadeonSoftwareSlimmer.Models.PreInstall;

namespace RadeonSoftwareSlimmer.ViewModels
{
    public class PreInstallViewModel : INotifyPropertyChanged
    {
        private bool _loadedPanelEnabled;


        public PreInstallViewModel()
        {
            LoadedPanelEnabled = false;

            InstallerFiles = new InstallerFilesModel();
            PackageList = new PackageListModel();
            ScheduledTaskList = new ScheduledTaskXmlListModel();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public InstallerFilesModel InstallerFiles { get; }
        public PackageListModel PackageList { get; }
        public ScheduledTaskXmlListModel ScheduledTaskList { get; }
        public bool LoadedPanelEnabled
        {
            get { return _loadedPanelEnabled; }
            private set
            {
                _loadedPanelEnabled = value;
                OnPropertyChanged(nameof(LoadedPanelEnabled));
            }
        }


        public async Task ExtractInstallerFilesAsync()
        {
            try
            {
                StaticViewModel.IsLoading = true;
                LoadedPanelEnabled = false;
                StaticViewModel.AddLogMessage("Extracting installer files");

                await Task.Run(() => InstallerFiles.ExtractInstallerFiles());

                ReadFromExtractedInstaller();

                StaticViewModel.AddLogMessage("Installer files extraction complete");
                LoadedPanelEnabled = true;
            }
            catch (Exception ex)
            {
                StaticViewModel.AddLogMessage(ex, "Extracting installer files failed");
            }
            finally
            {
                StaticViewModel.IsLoading = false;
            }
        }

        public void ReadFromExtractedInstaller()
        {
            try
            {
                StaticViewModel.IsLoading = true;
                LoadedPanelEnabled = false;
                StaticViewModel.AddLogMessage("Loading installer information");

                DirectoryInfo extractedDirectory = new DirectoryInfo(InstallerFiles.ExtractedInstallerDirectory);
                PackageList.LoadOrRefresh(extractedDirectory);
                ScheduledTaskList.LoadOrRefresh(extractedDirectory);

                StaticViewModel.AddLogMessage("Finished loading installer information");
                LoadedPanelEnabled = true;
            }
            catch (Exception ex)
            {
                StaticViewModel.AddLogMessage(ex, "Reading from extracted installer location failed");
            }
            finally
            {
                StaticViewModel.IsLoading = false;
            }
        }

        public void ModifyInstaller()
        {
            try
            {
                StaticViewModel.IsLoading = true;
                LoadedPanelEnabled = false;
                StaticViewModel.AddLogMessage("Modifying installer");

                foreach (PackageModel package in PackageList.InstallerPackages.Where(p => !p.Keep))
                {
                    PackageListModel.RemovePackage(package);
                }

                foreach (ScheduledTaskXmlModel task in ScheduledTaskList.ScheduledTasks.Where(t => !t.Enabled))
                {
                    ScheduledTaskXmlListModel.UnhideAndDisableScheduledTask(task);
                }

                ReadFromExtractedInstaller();

                StaticViewModel.AddLogMessage("Finished modifying installer");
                LoadedPanelEnabled = true;
            }
            catch (Exception ex)
            {
                StaticViewModel.AddLogMessage(ex, "Modifying installer failed");
            }
            finally
            {
                StaticViewModel.IsLoading = false;
            }
        }

        public void RunRadeonSoftwareSetup()
        {
            InstallerFiles.RunRadeonSoftwareSetup();
        }

        public void BrowseForInstallerFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Radeon Software Installers (*Radeon*.exe)|*Radeon*.exe|Executables (*.exe)|*.exe|All Files (*.*)|*.*";
            openFileDialog.CheckFileExists = true;
            openFileDialog.Multiselect = false;

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                FileInfo file = new FileInfo(openFileDialog.FileName);
                InstallerFiles.InstallerFile = openFileDialog.FileName;
                InstallerFiles.ExtractedInstallerDirectory = $@"{file.Directory}\{file.Name.Substring(0, file.Name.Length - file.Extension.Length)}";
            }
        }

        public void BrowseForExtractLocation()
        {
            //https://github.com/dotnet/wpf/issues/438  Why we can't have nice things and have to reference winforms :(
            using (System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                folderBrowserDialog.ShowNewFolderButton = true;

                if (Directory.Exists(InstallerFiles.ExtractedInstallerDirectory))
                    folderBrowserDialog.SelectedPath = InstallerFiles.ExtractedInstallerDirectory;

                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    InstallerFiles.ExtractedInstallerDirectory = folderBrowserDialog.SelectedPath;
                }
            }
        }
    }
}
