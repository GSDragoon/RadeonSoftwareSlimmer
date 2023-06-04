using System.IO.Abstractions;
using System.Windows;
using System.Windows.Controls;
using RadeonSoftwareSlimmer.Services;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Views
{
    public partial class PostInstallView : UserControl
    {
        private readonly PostInstallViewModel _viewModel = new PostInstallViewModel(new FileSystem(), new WindowsRegistry());


        public PostInstallView()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private async void UserControl_Initialized(object sender, System.EventArgs e)
        {
            await _viewModel.LoadOrRefreshAsync(false);
        }


        private async void btnLoadOrRefresh_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadOrRefreshAsync(true);
        }

        private async void btnApply_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.ApplyChangesAsync();
        }


        private async void btnHostServicesRestart_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.HostServices_RestartAsync();
        }

        private async void btnHostServicesStop_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.HostServices_StopAsync();
        }


        private void btnScheduledTaskEnableAll_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ScheduledTask_SetAll(true);
        }

        private void btnScheduledTaskDisableAll_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ScheduledTask_SetAll(false);
        }


        private async void btnServiceStart_Click(object sender, RoutedEventArgs e)
        {
            await PostInstallViewModel.Service_StartAsync(grdServices.SelectedItem);
        }

        private async void btnServiceStop_Click(object sender, RoutedEventArgs e)
        {
            await PostInstallViewModel.Service_StopAsync(grdServices.SelectedItem);
        }

        private async void btnServiceChangeStartMode_Click(object sender, RoutedEventArgs e)
        {
            await PostInstallViewModel.Service_SetStartModeAsync(grdServices.SelectedItem, cbxServiceStartMode.Text);
        }

        private async void btnServiceDelete_Click(object sender, RoutedEventArgs e)
        {
            await PostInstallViewModel.Service_DeleteAsync(grdServices.SelectedItem);
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
