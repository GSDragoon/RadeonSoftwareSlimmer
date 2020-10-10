using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using Microsoft.Win32;
using RadeonSoftwareSlimmer.Models;

namespace RadeonSoftwareSlimmer.ViewModels
{
    public static class StaticViewModel
    {
        private static LoggingModel _latestLogMessage;
        private static bool _isLoading;
        private static readonly object _logsLock = new object();

        static StaticViewModel()
        {
            IsLoading = false;
            BindingOperations.EnableCollectionSynchronization(Logs, _logsLock);
            LogToConsole = false;
        }


        public static event PropertyChangedEventHandler StaticPropertyChanged;
        public static void OnPropertyChanged(string propertyName) => StaticPropertyChanged?.Invoke(typeof(LoggingModel), new PropertyChangedEventArgs(propertyName));


        public static bool LogToConsole { get; set; } //Used with tests only
        public static ObservableCollection<LoggingModel> Logs { get; } = new ObservableCollection<LoggingModel>();
        public static string StatusMessage => _latestLogMessage?.Message;
        public static bool IsUiEnabled => !_isLoading;
        public static bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
                OnPropertyChanged(nameof(IsUiEnabled));
            }
        }

        public static void AddLogMessage(LoggingModel logMessage, bool updateStatus)
        {
            lock (_logsLock)
            {
                Logs.Add(logMessage);

                if (LogToConsole)
                    Console.WriteLine(logMessage);
            }

            if (updateStatus)
            {
                _latestLogMessage = logMessage;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public static void AddLogMessage(string logMessage) => AddLogMessage(new LoggingModel(logMessage), true);
        public static void AddLogMessage(Exception exception) => AddLogMessage(new LoggingModel(exception), true);
        public static void AddLogMessage(Exception exception, string logMessage) => AddLogMessage(new LoggingModel(exception, logMessage), true);

        public static void AddDebugMessage(string logMessage) => AddLogMessage(new LoggingModel(logMessage), false);
        public static void AddDebugMessage(Exception exception) => AddLogMessage(new LoggingModel(exception), false);
        public static void AddDebugMessage(Exception exception, string logMessage) => AddLogMessage(new LoggingModel(exception, logMessage), false);

        public static void ClearLogs()
        {
            lock (_logsLock)
            {
                Logs.Clear();
            }

            AddLogMessage("Cleared logging");
        }

        public static void SaveLogs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Log Files (*.log)|*.exe|All Files (*.*)|*.*";
            saveFileDialog.FileName = $"{nameof(RadeonSoftwareSlimmer)}_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.CurrentCulture)}.log";
            saveFileDialog.CheckFileExists = false;

            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                IsLoading = true;
                using (StreamWriter streamWriter = new StreamWriter(saveFileDialog.FileName, false))
                {
                    foreach (LoggingModel log in Logs)
                    {
                        streamWriter.WriteLine(log.ToString());
                    }

                    AddLogMessage($"Saved logging to {saveFileDialog.FileName}");
                }
            }

            IsLoading = false;
        }
    }
}
