using MahApps.Metro.Controls;
using RadeonSoftwareSlimmer.Services;

namespace RadeonSoftwareSlimmer
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "WPF event handlers cannot be static.")]
        private void btnLight_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ThemeService.SetTheme(ThemeService.Theme.Light);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "WPF event handlers cannot be static.")]
        private void btnDark_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ThemeService.SetTheme(ThemeService.Theme.Dark);
        }
    }
}
