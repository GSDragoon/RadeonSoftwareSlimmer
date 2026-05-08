using System;
using RadeonSoftwareSlimmer.Core.Enums;

namespace RadeonSoftwareSlimmer.Core.Interfaces
{
    // https://dahall.github.io/TaskScheduler/html/T_Microsoft_Win32_TaskScheduler_Task.htm
    public interface IScheduledTask : IDisposable
    {
        string Name { get; set; }
        string Description { get; set; }
        bool Enabled { get; set; }
        bool IsActive { get; set; }
        TaskState State { get; set; }
        string Command { get; set; }
        DateTime LastRunTime { get; set; }
        string Author { get; set; }

        void RegisterChanges();

        void Stop();
    }
}
