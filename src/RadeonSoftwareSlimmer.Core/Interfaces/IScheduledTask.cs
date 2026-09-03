using System;
using RadeonSoftwareSlimmer.Core.Enums;

namespace RadeonSoftwareSlimmer.Core.Interfaces
{
    // https://dahall.github.io/TaskScheduler/html/T_Microsoft_Win32_TaskScheduler_Task.htm
    public interface IScheduledTask
    {
        string Name { get; }
        string Description { get; }
        bool Enabled { get; }
        bool IsActive { get; }
        CoreTaskState State { get; }
        string Command { get; }
        DateTime LastRunTime { get; }
        string Author { get;  }

        void Enable();

        void Disable();
    }
}
