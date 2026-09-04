using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Win32.TaskScheduler;
using RadeonSoftwareSlimmer.Core.Enums;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Windows
{
    public class WindowsScheduledTask : IScheduledTask, INotifyPropertyChanged
    {
        private bool _enabled;
        private bool _active;
        private CoreTaskState _state;


        public WindowsScheduledTask(Task scheduledTask)
        {
            if (scheduledTask == null)
                throw new ArgumentNullException(nameof(scheduledTask), "Scheduled Task is null");

            Description = scheduledTask.Definition.RegistrationInfo.Description;
            Enabled = scheduledTask.Enabled;
            Name = scheduledTask.Name;
            IsActive = scheduledTask.IsActive;
            State = scheduledTask.State.ToCoreTaskState();
            Command = scheduledTask.Definition.Actions[0].ToString(CultureInfo.CurrentCulture);
            LastRunTime = scheduledTask.LastRunTime;
            Author = scheduledTask.Definition.RegistrationInfo.Author;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public string Name { get; }
        public string Description { get; }
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }

        public bool IsActive
        {
            get { return _active; }
            private set
            {
                _active = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }

        public CoreTaskState State
        {
            get { return _state; }
            private set
            {
                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }

        public string Command { get; }
        public DateTime LastRunTime { get; }
        public string Author { get; }

        public void Enable()
        {
            using (Task task = TaskService.Instance.GetTask(Name))
            {
                if (!task.Definition.Settings.Enabled)
                {
                    task.Definition.Settings.Enabled = true;

                    task.RegisterChanges();

                    IsActive = task.IsActive;
                    State = task.State.ToCoreTaskState();
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
                    task.Definition.Settings.Enabled = false;

                    task.RegisterChanges();

                    IsActive = task.IsActive;
                    State = task.State.ToCoreTaskState();
                    Enabled = task.Enabled;
                }
            }
        }
    }
}
