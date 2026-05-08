using System;
using System.ComponentModel;
using RadeonSoftwareSlimmer.Core.Interfaces;
using RadeonSoftwareSlimmer.Core.Enums;

namespace RadeonSoftwareSlimmer.Core.PostInstall
{
    public class ScheduledTaskModel : INotifyPropertyChanged
    {
        private readonly IAppLogger _logger;
        private readonly IScheduledTaskService _taskService;
        private bool _enabled;
        private bool _active;
        private TaskState _state;


        public ScheduledTaskModel(IScheduledTask task, IAppLogger logger, IScheduledTaskService taskService)
        {
            _logger = logger;
            _taskService = taskService;

            if (task == null)
                throw new ArgumentNullException(nameof(task), "Scheduled Task is null");

            Description = task.Description;
            Enabled = task.Enabled;
            Name = task.Name;
            Active = task.IsActive;
            State = task.State;
            Command = task.Command;
            LastRun = task.LastRunTime;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }
        public string Name { get; }
        public string Description { get; }
        public string Command { get; }
        public bool Active
        {
            get { return _active; }
            private set
            {
                _active = value;
                OnPropertyChanged(nameof(Active));
            }
        }
        public TaskState State
        {
            get { return _state; }
            private set
            {
                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }
        public DateTime LastRun { get; }


        public void Enable()
        {
            using (IScheduledTask task = _taskService.GetTask(Name))
            {
                if (!task.Enabled)
                {
                    _logger.Debug($"Enabling scheduled task {Name}");
                    task.Enabled = true;

                    task.RegisterChanges();

                    Active = task.IsActive;
                    State = task.State;
                    Enabled = task.Enabled;
                }
            }
        }

        public void Disable()
        {
            using (IScheduledTask task = _taskService.GetTask(Name))
            {
                if (task.Enabled)
                {
                    _logger.Debug($"Stopping scheduled task {Name}");
                    task.Stop();

                    _logger.Debug($"Disabling scheduled task {Name}");
                    task.Enabled = false;

                    task.RegisterChanges();

                    Active = task.IsActive;
                    State = task.State;
                    Enabled = task.Enabled;
                }
            }
        }
    }
}
