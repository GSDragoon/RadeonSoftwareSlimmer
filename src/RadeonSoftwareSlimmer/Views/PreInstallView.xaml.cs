using System.IO.Abstractions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Views
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class PreInstallView : UserControl
    {
        private readonly PreInstallViewModel _viewModel = new PreInstallViewModel(new FileSystem());


        public PreInstallView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private async Task UpdateWizardIndexAsync()
        {
            //Binding this doesn't work :(
            flpWizard.SelectedIndex = (int)_viewModel.FlipViewIndex;

            if (_viewModel.FlipViewIndex == PreInstallViewModel.WizardIndex.ExtractingInstaller)
            {
                await _viewModel.ExtractInstallerFilesAsync();
                await UpdateWizardIndexAsync();
            }

            if (_viewModel.FlipViewIndex == PreInstallViewModel.WizardIndex.ModifyInstaller)
            {
                _viewModel.ReadFromExtractedInstaller();
            }
        }


        private async void btnSkip0_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SkipInstallFile();
            await UpdateWizardIndexAsync();
        }

        private void btnInstallerFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.BrowseForInstallerFile();
        }

        private async void btnNext0_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ValidateInstallerFile();
            await UpdateWizardIndexAsync();
        }


        private async void btnBack1_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Back();
            await UpdateWizardIndexAsync();
        }

        private void btnExtractLocatonBrowse_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.BrowseForExtractLocation();
        }

        private async void btnNext1_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ValidateExtractLocation();
            await UpdateWizardIndexAsync();
        }


        private void btnModifyInstallerFiles_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ModifyInstaller();
        }

        private void btnRunInstaller_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RunRadeonSoftwareSetup();

            Window.GetWindow(this).Close();
        }

        private async void btnNewInstaller_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectNewInstaller();
            await UpdateWizardIndexAsync();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Stop these DataGrid SelectionChanged events from triggering the SelectionChanged even on the FlipView
            e.Handled = true;
        }


        private void btnPackageSelectAll_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Packages_SetAll(true);
        }
        private void btnPackageSelectNone_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Packages_SetAll(false);
        }

        private void btnScheduledTaskSelectAll_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ScheduledTask_SetAll(true);
        }
        private void btnScheduledTaskSelectNone_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ScheduledTask_SetAll(false);
        }

        private void btnDisplayComponentsSelectAll_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.DisplayComponents_SetAll(true);
        }
        private void btnDisplayComponentsSelectNone_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.DisplayComponents_SetAll(false);
        }
    }
}
