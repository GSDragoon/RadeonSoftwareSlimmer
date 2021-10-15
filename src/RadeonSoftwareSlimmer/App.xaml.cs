using System;
using System.Threading.Tasks;
using System.Windows;
using RadeonSoftwareSlimmer.Services;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SetupExceptionHandling();

            ThemeService.SetThemeToUserSettings();
        }


        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, ex) => LogUnhandledException((Exception)ex.ExceptionObject);

            DispatcherUnhandledException += (sender, ex) =>
            {
                LogUnhandledException(ex.Exception);
                ex.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (sender, ex) =>
            {
                LogUnhandledException(ex.Exception);
                ex.SetObserved();
            };
        }

        private static void LogUnhandledException(Exception exception)
        {
            StaticViewModel.AddLogMessage(exception);
            StaticViewModel.IsLoading = false;
        }
    }
}
