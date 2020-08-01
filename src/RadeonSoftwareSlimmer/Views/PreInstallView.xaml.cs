using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Views
{
    public partial class PreInstallView : UserControl
    {
        private readonly PreInstallViewModel _viewModel = new PreInstallViewModel();


        public PreInstallView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void UpdateWizardIndex()
        {
            //Binding this doesn't work :(
            flpWizard.SelectedIndex = (int)_viewModel.FlipViewIndex;
        }

        private async void flpWizard_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FlipView flipView = (FlipView)sender;

            if (flipView.SelectedIndex == (int)PreInstallViewModel.WizardIndex.ExtractingInstaller)
            {
                await _viewModel.ExtractInstallerFilesAsync();
                UpdateWizardIndex();
            }

            if (flipView.SelectedIndex == (int)PreInstallViewModel.WizardIndex.ModifyInstaller)
            {
                _viewModel.ReadFromExtractedInstaller();
            }
        }


        private void btnSkip0_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SkipInstallFile();
            UpdateWizardIndex();
        }

        private void btnInstallerFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.BrowseForInstallerFile();
        }

        private void btnNext0_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ValidateInstallerFile();
            UpdateWizardIndex();
        }


        private void btnBack1_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Back();
            UpdateWizardIndex();
        }

        private void btnExtractLocatonBrowse_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.BrowseForExtractLocation();
        }

        private void btnNext1_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ValidateExtractLocation();
            UpdateWizardIndex();
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

        private void btnNewInstaller_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectNewInstaller();
            UpdateWizardIndex();
        }
    }
}
