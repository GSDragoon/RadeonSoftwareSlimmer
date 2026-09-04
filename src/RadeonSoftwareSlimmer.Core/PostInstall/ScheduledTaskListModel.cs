using System;
using System.Collections.Generic;
using System.ComponentModel;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Core.PostInstall
{
    public class ScheduledTaskListModel : INotifyPropertyChanged
    {
        private readonly IAppLogger _logger;
        private readonly IScheduledTaskService _taskService;
        private IEnumerable<ScheduledTaskModel> _scheduledTasks;


        public ScheduledTaskListModel(IAppLogger logger, IScheduledTaskService taskService)
        {
            _logger = logger;
            _taskService = taskService;
        }


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


        private IEnumerable<ScheduledTaskModel> GetAllRadeonScheduledTasks()
        {
            foreach (IScheduledTask task in _taskService.FindAllTasks(t => IsRadeonTask(t), false))
            {
                yield return new ScheduledTaskModel(task, _logger, _taskService);
            }
        }

        private bool IsRadeonTask(IScheduledTask scheduledTask)
        {
            string author = scheduledTask.Author;
            if (!string.IsNullOrWhiteSpace(author)
                && author.Equals("Advanced Micro Devices", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string name = scheduledTask.Name;
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (name.Equals("DVRAnalytics", StringComparison.OrdinalIgnoreCase)
                || name.Equals("StartAUEP", StringComparison.OrdinalIgnoreCase)
                || name.Equals("StartCN", StringComparison.OrdinalIgnoreCase)
                || name.Equals("StartCNBM", StringComparison.OrdinalIgnoreCase)
                || name.Equals("StartDVR", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Debug($"Found scheduled task {name}");
                return true;
            }


            return false;
        }
    }
}
