using System;
using System.Diagnostics;
using System.Threading;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Windows
{
    public class ProcessHandler : IProcessHandler
    {
        private readonly IAppLogger _logger;


        public ProcessHandler(IAppLogger logger)
        {
            _logger = logger;
        }


        public bool IsProcessRunning(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
                return false;

            return Process.GetProcessesByName(processName).Length > 0;
        }

        public void WaitForProcessToEnd(string processName, int maxWaitSeconds)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (IsProcessRunning(processName))
            {
                if (sw.ElapsedMilliseconds >= maxWaitSeconds * 1000)
                {
                    foreach (Process process in Process.GetProcessesByName(processName))
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Unable to stop process [{process.Id}] {process.ProcessName}");
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }

        public void WaitForProcessToStart(string processName, int maxWaitSeconds)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (!IsProcessRunning(processName))
            {
                if (sw.ElapsedMilliseconds >= maxWaitSeconds * 1000)
                {
                    _logger.Debug($"Process {processName} did not start");
                    return;
                }

                Thread.Sleep(1000);
            }
        }
    }
}
