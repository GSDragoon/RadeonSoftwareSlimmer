using System.Windows;
using System.Windows.Controls;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Views
{
    /// <summary>
    /// Interaction logic for PostInstallView.xaml
    /// </summary>
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
    }
}
