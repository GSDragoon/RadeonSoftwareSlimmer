namespace RadeonSoftwareSlimmer.Core.Enums
{
    // https://dahall.github.io/TaskScheduler/html/T_Microsoft_Win32_TaskScheduler_TaskState.htm
    public enum TaskState : int
    {
        Unknown = 0,
        Disabled = 1,
        Queued = 2,
        Ready = 3,
        Running = 4
    }
}
