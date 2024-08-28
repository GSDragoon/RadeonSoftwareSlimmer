using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Views
{
    public partial class LoggingView : System.Windows.Controls.UserControl
    {
        public LoggingView()
        {
            InitializeComponent();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "WPF event handlers cannot be static.")]
        private void btnSaveLogs_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StaticViewModel.SaveLogs();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "WPF event handlers cannot be static.")]
        private void btnClearLogs_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StaticViewModel.ClearLogs();
        }
    }
}
