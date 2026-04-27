namespace RadeonSoftwareSlimmer.Core.Interfaces
{
    public interface IProcessHandler
    {
        bool IsProcessRunning(string processName);

        void WaitForProcessToEnd(string processName, int maxWaitSeconds);

        void WaitForProcessToStart(string processName, int maxWaitSeconds);
    }
}
