using System;
using RadeonSoftwareSlimmer.Core.Enums;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Core.Test.TestDoubles
{
    public class FakeScheduledTask : IScheduledTask
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public bool IsActive { get; set; }
        public CoreTaskState State { get; set; }
        public string Command { get; set; }
        public DateTime LastRunTime { get; set; }
        public string Author { get; set; }

        public int EnableCalls { get; private set; }
        public int DisableCalls { get; private set; }


        public void Enable()
        {
            EnableCalls++;
            Enabled = true;
            State = CoreTaskState.Ready;
        }

        public void Disable()
        {
            DisableCalls++;
            Enabled = false;
            IsActive = false;
            State = CoreTaskState.Disabled;
        }
    }
}
