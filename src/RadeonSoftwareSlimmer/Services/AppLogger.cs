using System;
using RadeonSoftwareSlimmer.Core.Interfaces;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Services
{
    public class AppLogger : IAppLogger
    {
        public bool IsLoading
        {
            get => StaticViewModel.IsLoading;
            set => StaticViewModel.IsLoading = value;
        }


        public void Info(string message) => StaticViewModel.AddLogMessage(message);
        public void Info(Exception ex) => StaticViewModel.AddLogMessage(ex);
        public void Info(Exception ex, string message) => StaticViewModel.AddLogMessage(ex, message);

        public void Debug(string message) => StaticViewModel.AddDebugMessage(message);
        public void Debug(Exception ex) => StaticViewModel.AddDebugMessage(ex);
        public void Debug(Exception ex, string message) => StaticViewModel.AddDebugMessage(ex, message);

        public void Error(Exception ex) => StaticViewModel.AddLogMessage(ex);
        public void Error(Exception ex, string message) => StaticViewModel.AddLogMessage(ex, message);
    }
}
