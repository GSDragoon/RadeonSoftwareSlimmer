using System.Windows;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Views
{
    /// <summary>
    /// Interaction logic for PreInstallView.xaml
    /// </summary>
    public partial class PreInstallView : System.Windows.Controls.UserControl
    {
        private readonly PreInstallViewModel _viewModel = new PreInstallViewModel();


        public PreInstallView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }


        private void btnInstallerFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.BrowseForInstallerFile();
        }

        private void btnExtractLocatonBrowse_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.BrowseForExtractLocation();
        }

        private async void btnExtractInstallerFiles_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.ExtractInstallerFilesAsync();
        }

        private void btnReadInstallerFiles_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ReadFromExtractedInstaller();
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
    }
}
