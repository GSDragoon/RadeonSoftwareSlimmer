namespace RadeonSoftwareSlimmer.Core.Enums
{
    // https://learn.microsoft.com/en-us/dotnet/api/system.serviceprocess.servicecontrollerstatus
    public enum ServiceControllerStatus : int
    {
        Stopped = 1,
        StartPending = 2,
        StopPending = 3,
        Running = 4,
        ContinuePending = 5,
        PausePending = 6,
        Paused = 7
    }
}
