using System;

namespace RadeonSoftwareSlimmer.Core.Interfaces
{
    // https://dahall.github.io/TaskScheduler/html/T_Microsoft_Win32_TaskScheduler_TaskService.htm
    public interface IScheduledTaskService
    {
        IScheduledTask GetTask(string taskName);

        IScheduledTask[] FindAllTasks(Predicate<IScheduledTask> filter, bool searchAllFolders = true);
    }
}
