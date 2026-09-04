using System;
using RadeonSoftwareSlimmer.Core.Enums;

namespace RadeonSoftwareSlimmer.Core.Interfaces
{
    // https://learn.microsoft.com/en-us/dotnet/api/system.serviceprocess.servicecontroller
    public interface IServiceController : IDisposable
    {
        string ServiceName { get; }
        bool Exists { get; }
        string DisplayName { get; }
        CoreServiceStartMode StartType { get; }
        CoreServiceType ServiceType { get; }
        CoreServiceControllerStatus Status { get; }

        void Start();
        void Stop();
        void Refresh();
        void WaitForStatus(CoreServiceControllerStatus desiredStatus, TimeSpan timeout);
    }
}
