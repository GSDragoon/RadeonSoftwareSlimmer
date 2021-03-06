﻿using System.Windows;
using System.Windows.Controls;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Views
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
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
