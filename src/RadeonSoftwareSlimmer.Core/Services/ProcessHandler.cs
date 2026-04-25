using System;
using System.Diagnostics;
using System.Threading;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Core.Services
{
    public class ProcessHandler : IProcessHandler
    {
        private readonly IAppLogger _logger;
        private readonly string _processName;


        public ProcessHandler(string processName, IAppLogger logger)
        {
            _logger = logger;
            _processName = processName;
        }


        public bool IsProcessRunning()
        {
            if (string.IsNullOrWhiteSpace(_processName))
                return false;

            return Process.GetProcessesByName(_processName).Length > 0;
        }

        public void WaitForProcessToEnd(int maxWaitSeconds)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (IsProcessRunning())
            {
                if (sw.ElapsedMilliseconds >= maxWaitSeconds * 1000)
                {
                    foreach (Process process in Process.GetProcessesByName(_processName))
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

        public void WaitForProcessToStart(int maxWaitSeconds)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (!IsProcessRunning())
            {
                if (sw.ElapsedMilliseconds >= maxWaitSeconds * 1000)
                {
                    _logger.Debug($"Process {_processName} did not start");
                    return;
                }

                Thread.Sleep(1000);
            }
        }
    }
}
