using System.Windows;
using System.Windows.Controls;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Views
{
    public partial class PostInstallView : UserControl
    {
        private readonly PostInstallViewModel _viewModel = new PostInstallViewModel();


        public PostInstallView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }


        private void btnLoadOrRefresh_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadOrRefresh();
        }

        private async void btnApply_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.ApplyyChangesAsync();
        }

        private void btnTempFilesSelectAll_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.TempFilesSetAll(true);
        }

        private void btnTempFilesSelectNone_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.TempFilesSetAll(false);
        }
    }
}
