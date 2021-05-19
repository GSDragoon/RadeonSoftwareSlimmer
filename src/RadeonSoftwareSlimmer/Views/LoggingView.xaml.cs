using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Views
{
    public partial class LoggingView : System.Windows.Controls.UserControl
    {
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public LoggingView()
        {
            InitializeComponent();
        }

        private void btnSaveLogs_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StaticViewModel.SaveLogs();
        }

        private void btnClearLogs_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StaticViewModel.ClearLogs();
        }
    }
}
