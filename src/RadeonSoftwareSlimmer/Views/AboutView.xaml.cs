using System;
using System.Diagnostics;
using System.Windows.Navigation;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Views
{
    public partial class AboutView : System.Windows.Controls.UserControl
    {
        public AboutView()
        {
            InitializeComponent();

            lblVersion.Text = $"Version: {System.Windows.Forms.Application.ProductVersion} (.NET Version: {Environment.Version})";
            StaticViewModel.AddDebugMessage($"{nameof(RadeonSoftwareSlimmer)} version {System.Windows.Forms.Application.ProductVersion} (.NET version {Environment.Version})");
        }


        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(e.Uri.AbsoluteUri);
            startInfo.UseShellExecute = true;

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();
            }

            e.Handled = true;
        }
    }
}
