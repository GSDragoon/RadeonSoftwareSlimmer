using System;
using System.Diagnostics;
using System.Linq;

namespace RadeonSoftwareSlimmer.Models.PostInstall
{
    public class RunningProcessModel
    {
        public RunningProcessModel() { }

        public RunningProcessModel(string fileName, string description)
        {
            Name = fileName;
            Description = description;
            ProcessType = "Process";

            Refresh();
        }


        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public string ProcessType { get; protected set; }

        public int ProcessId { get; protected set; }
        public bool IsRunning { get; protected set; }

        public DateTime StartTime { get; protected set; }
        public TimeSpan CpuTime { get; protected set; }
        public long WorkingSet { get; protected set; }
        public long PrivateBytes { get; protected set; }


        public void Refresh()
        {
            Process[] processes = Process.GetProcessesByName(Name);
            if (processes.Length > 0)
            {
                using (Process p = processes.First())
                {
                    ProcessId = p.Id;
                    IsRunning = true;
                    StartTime = p.StartTime;
                    CpuTime = p.TotalProcessorTime;
                    WorkingSet = p.WorkingSet64;
                    PrivateBytes = p.PrivateMemorySize64;
                }
            }
        }

        public virtual void Disable() { }

        public virtual void Restart() { }

        public virtual void Stop()
        {
            using (Process process = Process.GetProcessById(ProcessId))
            {
                process.Kill();
            }
            Refresh();
        }
    }
}
