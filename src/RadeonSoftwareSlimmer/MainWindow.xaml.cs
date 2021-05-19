using MahApps.Metro.Controls;
using RadeonSoftwareSlimmer.Services;

namespace RadeonSoftwareSlimmer
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLight_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ThemeService.SetTheme(ThemeService.Theme.Light);
        }

        private void btnDark_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ThemeService.SetTheme(ThemeService.Theme.Dark);
        }
    }
}
