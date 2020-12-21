using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Win32.TaskScheduler;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Models.PostInstall
{
    public class ScheduledTaskListModel : INotifyPropertyChanged
    {
        private IEnumerable<ScheduledTaskModel> _scheduledTasks;


        public ScheduledTaskListModel() { }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public IEnumerable<ScheduledTaskModel> RadeonScheduledTasks
        {
            get { return _scheduledTasks; }
            set
            {
                _scheduledTasks = value;
                OnPropertyChanged(nameof(RadeonScheduledTasks));
            }
        }


        public void LoadOrRefresh()
        {
            RadeonScheduledTasks = new List<ScheduledTaskModel>(GetAllRadeonScheduledTasks());
        }

        public void ApplyChanges()
        { 
            foreach (ScheduledTaskModel scheduledTask in _scheduledTasks)
            {
                if (scheduledTask.Enabled)
                    scheduledTask.Enable();
                else
                    scheduledTask.Disable();
            }
        }


        private static IEnumerable<ScheduledTaskModel> GetAllRadeonScheduledTasks()
        {
            foreach (Task task in TaskService.Instance.FindAllTasks(t => IsRadeonTask(t), false))
            {
                yield return new ScheduledTaskModel(task);
            }
        }

        private static bool IsRadeonTask(Task scheduledTask)
        {
            string author = scheduledTask.Definition.RegistrationInfo.Author;
            if (!string.IsNullOrWhiteSpace(author)
                && author.Equals("Advanced Micro Devices", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string name = scheduledTask.Name;
            if (!string.IsNullOrWhiteSpace(name)
                && (
                    name.Equals("StartCN", StringComparison.OrdinalIgnoreCase)
                    || name.Equals("StartDVR", StringComparison.OrdinalIgnoreCase)
                    || name.Equals("StartCNBM", StringComparison.OrdinalIgnoreCase)))
            {
                StaticViewModel.AddDebugMessage($"Found scheduled task {name}");
                return true;
            }


            return false;
        }
    }
}
