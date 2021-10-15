using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace RadeonSoftwareSlimmer.Views
{
    public partial class HelpLink : UserControl
    {
        public static readonly DependencyProperty LinkProperty = DependencyProperty.Register(nameof(Link), typeof(Uri), typeof(MainWindow));


        public HelpLink()
        {
            InitializeComponent();
        }


        public Uri Link
        {
            get { return (Uri)GetValue(LinkProperty); }
            set
            {
                SetValue(LinkProperty, value);
                btnLink.ToolTip = $"Click to open wiki page for help on this topic: {value?.AbsoluteUri}";
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(Link.AbsoluteUri);
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
