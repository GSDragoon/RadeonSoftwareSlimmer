using System;
using System.Diagnostics.CodeAnalysis;
using RadeonSoftwareSlimmer.Core.Enums;
using RadeonSoftwareSlimmer.Core.Interfaces;

namespace RadeonSoftwareSlimmer.Core.Test.TestDoubles
{
    [SuppressMessage("Design", "S3881:Fix this implementation of 'IDisposable' to conform to the dispose pattern.", Justification = "Test double with no unmanaged resources; the full dispose pattern is unnecessary.")]
    public class FakeServiceController : IServiceController
    {
        public string ServiceName { get; set; } = string.Empty;
        public bool Exists { get; set; } = true;
        public string DisplayName { get; set; }
        public CoreServiceStartMode StartType { get; set; }
        public CoreServiceType ServiceType { get; set; }
        public CoreServiceControllerStatus Status { get; set; }

        public int StartCalls { get; private set; }
        public int StopCalls { get; private set; }
        public int RefreshCalls { get; private set; }
        public bool Disposed { get; private set; }


        public void Start()
        {
            StartCalls++;
            Status = CoreServiceControllerStatus.Running;
        }

        public void Stop()
        {
            StopCalls++;
            Status = CoreServiceControllerStatus.Stopped;
        }

        public void Refresh()
        {
            RefreshCalls++;
        }

        // Real ServiceController blocks until the status is observed or the timeout elapses; the fake assumes the desired status is reached immediately.
        public void WaitForStatus(CoreServiceControllerStatus desiredStatus, TimeSpan timeout)
        {
            Status = desiredStatus;
        }


        public void Dispose()
        {
            Disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
