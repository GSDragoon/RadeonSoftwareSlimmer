using System;
using System.Linq;
using Microsoft.Win32.TaskScheduler;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Windows
{
    public class WindowsScheduledTaskService : IScheduledTaskService
    {
        public WindowsScheduledTaskService() { }

        public IScheduledTask[] FindAllTasks(Predicate<IScheduledTask> filter, bool searchAllFolders = true)
        {
            return TaskService.Instance
                .FindAllTasks(filter.ToWindowsTaskPredicate(), searchAllFolders)
                .Select(t => t.ToWindowsScheduledTask())
                .ToArray();
        }

        public IScheduledTask GetTask(string taskName)
        {
            return new WindowsScheduledTask(TaskService.Instance.GetTask(taskName));
        }
    }
}
