using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.Win32.TaskScheduler;
using RadeonSoftwareSlimmer.ViewModels;

namespace RadeonSoftwareSlimmer.Models.PostInstall
{
    public class ScheduledTaskModel : INotifyPropertyChanged
    {
        private bool _enabled;
        private bool _active;
        private TaskState _state;


        public ScheduledTaskModel(Task scheduledTask)
        {
            if (scheduledTask == null)
                throw new ArgumentNullException(nameof(scheduledTask), "Scheduled Task is null");

            Description = scheduledTask.Definition.RegistrationInfo.Description;
            Enabled = scheduledTask.Enabled;
            Name = scheduledTask.Name;
            Active = scheduledTask.IsActive;
            State = scheduledTask.State;
            Command = scheduledTask.Definition.Actions.First().ToString(CultureInfo.CurrentCulture);
            LastRun = scheduledTask.LastRunTime;
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
            using (Task task = TaskService.Instance.GetTask(Name))
            {
                if (!task.Definition.Settings.Enabled)
                {
                    StaticViewModel.AddDebugMessage($"Enabling scheduled task {Name}");
                    task.Definition.Settings.Enabled = true;

                    task.RegisterChanges();

                    Active = task.IsActive;
                    State = task.State;
                    Enabled = task.Enabled;
                }
            }
        }

        public void Disable()
        {
            using (Task task = TaskService.Instance.GetTask(Name))
            {
                if (task.Definition.Settings.Enabled)
                {
                    StaticViewModel.AddDebugMessage($"Stopping scheduled task {Name}");
                    task.Stop();

                    StaticViewModel.AddDebugMessage($"Disabling scheduled task {Name}");
                    task.Definition.Settings.Enabled = false;

                    task.RegisterChanges();

                    Active = task.IsActive;
                    State = task.State;
                    Enabled = task.Enabled;
                }
            }
        }
    }
}
