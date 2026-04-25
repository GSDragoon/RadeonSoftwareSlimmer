namespace RadeonSoftwareSlimmer.Core.Interfaces
{
    public interface IProcessHandler
    {
        bool IsProcessRunning();

        void WaitForProcessToEnd(int maxWaitSeconds);

        void WaitForProcessToStart(int maxWaitSeconds);
    }
}
