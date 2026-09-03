using System.Collections.Generic;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Core.Test.TestDoubles
{
    public class FakeProcessHandler : IProcessHandler
    {
        public ISet<string> RunningProcesses { get; } = new HashSet<string>();

        public bool IsProcessRunning(string processName)
        {
            return !string.IsNullOrWhiteSpace(processName) && RunningProcesses.Contains(processName);
        }

        public void WaitForProcessToEnd(string processName, int maxWaitSeconds)
        {
            RunningProcesses.Remove(processName);
        }

        public void WaitForProcessToStart(string processName, int maxWaitSeconds)
        {
            RunningProcesses.Add(processName);
        }
    }
}
