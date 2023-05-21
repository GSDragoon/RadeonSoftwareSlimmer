using System;
using System.ComponentModel;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using RadeonSoftwareSlimmer.Models.PreInstall;

namespace RadeonSoftwareSlimmer.ViewModels
{
    public class PreInstallViewModel : INotifyPropertyChanged
    {
        private readonly IFileSystem _fileSystem;

        public PreInstallViewModel(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            FlipViewIndex = WizardIndex.SelectInstaller;
            InstallerAlreadyExtracted = false;

            InstallerFiles = new InstallerFilesModel(_fileSystem);
            PackageList = new PackageListModel(_fileSystem);
            ScheduledTaskList = new ScheduledTaskXmlListModel(_fileSystem);
            DisplayComponentList = new DisplayComponentListModel(_fileSystem);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public InstallerFilesModel InstallerFiles { get; }
        public PackageListModel PackageList { get; }
        public ScheduledTaskXmlListModel ScheduledTaskList { get; }
        public DisplayComponentListModel DisplayComponentList { get; }
        public bool InstallerAlreadyExtracted { get; set; }
        public WizardIndex FlipViewIndex { get; set; }


        public enum WizardIndex : int
        {
            Empty = -1,
            SelectInstaller = 0,
            SelectExtractLocation = 1,
            ExtractingInstaller = 2,
            ModifyInstaller = 3,
            InstallerDone = 4,
        }


        public void SkipInstallFile()
        {
            InstallerAlreadyExtracted = true;
            FlipViewIndex = WizardIndex.SelectExtractLocation;
        }

        public void ValidateInstallerFile()
        {
            if (InstallerFiles.ValidateInstallerFile())
            {
                FlipViewIndex = WizardIndex.SelectExtractLocation;
            }
        }

        public void BrowseForInstallerFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Radeon Software Installers (*radeon*.exe;*adrenalin*.exe)|*radeon*.exe;*adrenalin*.exe|Executables (*.exe)|*.exe|All Files (*.*)|*.*";
            openFileDialog.CheckFileExists = true;
            openFileDialog.Multiselect = false;

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                IFileInfo file = _fileSystem.FileInfo.New(openFileDialog.FileName);
                InstallerFiles.InstallerFile = openFileDialog.FileName;
                InstallerFiles.ExtractedInstallerDirectory = $@"{file.Directory}\{file.Name.Substring(0, file.Name.Length - file.Extension.Length)}";
            }
        }


        public void Back()
        {
            FlipViewIndex = WizardIndex.SelectInstaller;
            InstallerAlreadyExtracted = false;
        }

        public void ValidateExtractLocation()
        {
            if (InstallerAlreadyExtracted && InstallerFiles.ValidateExtractedLocation())
            {
                FlipViewIndex = WizardIndex.ModifyInstaller;
            }
            else if (InstallerFiles.ValidatePreExtractLocation())
            {
                FlipViewIndex = WizardIndex.ExtractingInstaller;
            }
        }

        public void BrowseForExtractLocation()
        {
            //https://github.com/dotnet/wpf/issues/438  Why we can't have nice things and have to reference winforms :(
            using (System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                folderBrowserDialog.ShowNewFolderButton = true;

                if (_fileSystem.Directory.Exists(InstallerFiles.ExtractedInstallerDirectory))
                    folderBrowserDialog.SelectedPath = InstallerFiles.ExtractedInstallerDirectory;

                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    InstallerFiles.ExtractedInstallerDirectory = folderBrowserDialog.SelectedPath;
                }
            }
        }


        public async Task ExtractInstallerFilesAsync()
        {
            try
            {
                StaticViewModel.IsLoading = true;
                StaticViewModel.AddLogMessage("Extracting installer files");

                await Task.Run(() => InstallerFiles.ExtractInstallerFiles());
                FlipViewIndex = WizardIndex.ModifyInstaller;

                StaticViewModel.AddLogMessage("Installer files extraction complete");
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


        public void SelectNewInstaller()
        {
            InstallerFiles.InstallerFile = string.Empty;
            InstallerFiles.ExtractedInstallerDirectory = string.Empty;
            InstallerAlreadyExtracted = false;
            FlipViewIndex = WizardIndex.SelectInstaller;
        }

        public void ReadFromExtractedInstaller()
        {
            try
            {
                StaticViewModel.IsLoading = true;
                StaticViewModel.AddLogMessage("Loading installer information");

                IDirectoryInfo extractedDirectory = _fileSystem.DirectoryInfo.New(InstallerFiles.ExtractedInstallerDirectory);
                PackageList.LoadOrRefresh(extractedDirectory);
                ScheduledTaskList.LoadOrRefresh(extractedDirectory);
                DisplayComponentList.LoadOrRefresh(extractedDirectory.FullName);

                StaticViewModel.AddLogMessage("Finished loading installer information");
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

        public void Packages_SetAll(bool keep)
        {
            foreach (PackageModel package in PackageList.InstallerPackages)
            {
                package.Keep = keep;
            }
        }

        public void ScheduledTask_SetAll(bool enabled)
        {
            foreach (ScheduledTaskXmlModel scheduledTask in ScheduledTaskList.ScheduledTasks)
            {
                scheduledTask.Enabled = enabled;
            }
        }

        public void DisplayComponents_SetAll(bool keep)
        {
            foreach (DisplayComponentModel displayComponent in DisplayComponentList.DisplayDriverComponents)
            {
                displayComponent.Keep = keep;
            }
        }

        public void ModifyInstaller()
        {
            try
            {
                StaticViewModel.IsLoading = true;
                StaticViewModel.AddLogMessage("Modifying installer");

                foreach (PackageModel package in PackageList.InstallerPackages.Where(p => !p.Keep))
                {
                    PackageListModel.RemovePackage(package);
                }

                foreach (ScheduledTaskXmlModel task in ScheduledTaskList.ScheduledTasks)
                {
                    ScheduledTaskList.SetScheduledTaskStatusAndUnhide(task);
                }

                DisplayComponentList.RemoveComponentsNotKeeping();

                ReadFromExtractedInstaller();

                StaticViewModel.AddLogMessage("Finished modifying installer");
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

        public void ResetInstallerToDefaults()
        {
            try
            {
                StaticViewModel.IsLoading = true;
                StaticViewModel.AddLogMessage("Resetting installer to defaults");

                PackageList.RestoreToDefault();

                ReadFromExtractedInstaller();

                StaticViewModel.AddLogMessage("Finished resetting installer to defaults");
            }
            catch (Exception ex)
            {
                StaticViewModel.AddLogMessage(ex, "Resetting installer to defaults failed");
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

        public void RunAmdCleanupUtility()
        {
            InstallerFiles.RunAmdCleanupUtility();
        }
    }
}
